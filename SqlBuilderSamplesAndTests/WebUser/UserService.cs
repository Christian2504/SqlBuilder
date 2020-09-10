using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SqlBuilderFramework;
using Microsoft.IdentityModel.Tokens;
using Kendo.DynamicLinqCore;
using System.Collections.Generic;

namespace SqlBuilderSamplesAndTests
{
    public class UserService
    {
        private const string _secret = "ANd HeRe we go. 4 ThIS pROjEct we neEd a gOoD SeCrET ConfiG!!"; // From AppSettings

        private readonly IDatabase _database;

        public UserService(IDatabase database)
        {
            _database = database;
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress)
        {
            var tblRefreshToken = new TblRefreshToken();

            var query = _database.Select
                .From(Tables.Users)
                .Where(Tables.Users.ColIsActive == true & Tables.Users.ColUsername == model.Username);

            var user = query.ReadAll(User.Mapper).SingleOrDefault();

            if (user == null)
                return null;

            if (!CryptVerify(model.Password, user.Password))
                return null;

            // authentication successful so generate jwt and refresh tokens
            var jwtToken = generateJwtToken(user);
            var refreshToken = generateRefreshToken(ipAddress);

            // save refresh token
            _database.Insert
                .In(tblRefreshToken)
                .Set(tblRefreshToken.ColToken.To(refreshToken.Token),
                     tblRefreshToken.ColUserId.To(user.Id),
                     tblRefreshToken.ColExpires.To(refreshToken.Expires),
                     tblRefreshToken.ColCreated.To(refreshToken.Created),
                     tblRefreshToken.ColCreatedByIp.To(refreshToken.CreatedByIp))
                .Execute();

            return new AuthenticateResponse(user, jwtToken, refreshToken.Token);
        }

        public AuthenticateResponse RefreshToken(string token, string ipAddress)
        {
            var refreshToken = new RefreshToken();
            var user = new User();

            var tblRefreshToken = new TblRefreshToken();
            var tblUsers = new TblUsers();

            var query = _database.Select
                .From(tblRefreshToken.InnerJoin(tblUsers, tblRefreshToken.ColUserId == tblUsers.ColId))
                .Where(tblRefreshToken.ColToken == token);

            query.Map(tblRefreshToken.ColToken, value => refreshToken.Token = value);
            query.Map(tblRefreshToken.ColExpires, value => refreshToken.Expires = value);
            query.Map(tblRefreshToken.ColRevoked, value => refreshToken.Revoked = value);

            query.Map(tblRefreshToken.ColUserId, value => user.Id = value);
            query.Map(tblUsers.ColFirstName, value => user.FirstName = value);
            query.Map(tblUsers.ColLastName, value => user.LastName = value);
            query.Map(tblUsers.ColUsername, value => user.Username = value);
            query.Map(tblUsers.ColRole, value => user.Role = value);

            query.ReadSingle();

            if (user.Id == 0 || !refreshToken.IsActive) // return null if token is no longer active
                return null;

            // replace old refresh token with a new one and save
            var newRefreshToken = generateRefreshToken(ipAddress);
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;

            // update old Refresh Token
            _database.Update.In(tblRefreshToken).Where(tblRefreshToken.ColToken == token)
                .Set(tblRefreshToken.ColReplacedByToken.To(refreshToken.ReplacedByToken),
                    tblRefreshToken.ColRevoked.To(refreshToken.Revoked),
                    tblRefreshToken.ColRevokedByIp.To(refreshToken.RevokedByIp))
                .Execute();

            // insert new Refresh Token
            _database.Insert
                .In(tblRefreshToken)
                .Set(tblRefreshToken.ColToken.To(newRefreshToken.Token),
                    tblRefreshToken.ColUserId.To(user.Id),
                    tblRefreshToken.ColExpires.To(newRefreshToken.Expires),
                    tblRefreshToken.ColCreated.To(newRefreshToken.Created),
                    tblRefreshToken.ColCreatedByIp.To(newRefreshToken.CreatedByIp))
                .Execute();

            // generate new jwt
            var jwtToken = generateJwtToken(user);

            return new AuthenticateResponse(user, jwtToken, newRefreshToken.Token);
        }

        public bool RevokeToken(string token, string ipAddress)
        {
            var tblRefreshToken = new TblRefreshToken();

            var queryToken = _database.Select
                .From(tblRefreshToken)
                .Where(tblRefreshToken.ColToken == token);

            var refreshToken = queryToken.ReadAll(new DbMapper<RefreshToken>()
                .Map(tblRefreshToken.ColToken, (rt, value) => rt.Token = value)
                .Map(tblRefreshToken.ColExpires, (rt, value) => rt.Expires = value)
                .Map(tblRefreshToken.ColRevoked, (rt, value) => rt.Revoked = value)
                ).SingleOrDefault();

            if (refreshToken == null || !refreshToken.IsActive) // return false if token is not active
                return false;

            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;

            _database.Update
                .In(tblRefreshToken)
                .Where(tblRefreshToken.ColToken == token)
                .Set(tblRefreshToken.ColRevoked.To(refreshToken.Revoked),
                     tblRefreshToken.ColRevokedByIp.To(refreshToken.RevokedByIp))
                .Execute();

            return true;
        }

        public List<User> GetAll()
        {
            var query = _database.Select
                .From(Tables.Users)
                .Where(Tables.Users.ColIsActive == true)
                .OrderBy(Tables.Users.ColId);

            return query.ReadAll(User.Mapper);
        }

        public DataSourceResult GetByRequest(DataSourceRequest request)
        {
            if (request == null)
                return null;

            var query = _database.Select
                .From(Tables.Users)
                .Where(Tables.Users.ColIsActive == true);

            var columnMap = new Dictionary<string, DbColumn>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(User.Id)] = Tables.Users.ColId,
                [nameof(User.FirstName)] = Tables.Users.ColFirstName,
                [nameof(User.LastName)] = Tables.Users.ColLastName,
                [nameof(User.Username)] = Tables.Users.ColUsername,
                [nameof(User.Role)] = Tables.Users.ColRole,
            };

            DataSourceRequestExtensions.ApplyToQuery(query, columnMap, request);

            if (!query.IsOrdered)
                query.OrderBy(Tables.Users.ColId);

            return new DataSourceResult
            {
                Data = query.ReadAll(User.Mapper),
                Total = _database.Select.From(Tables.Users).Where(query.Constraint).ReadValue(Tables.Users.ColId.Count()) ?? 0
            };
        }

        public User GetById(int id)
        {
            var query = _database.Select
                .From(Tables.Users)
                .Where(Tables.Users.ColId == id);

            return query.ReadAll(User.Mapper).SingleOrDefault();
        }

        public void Add(UserNew user)
        {
            var existingUserId = _database.Select
                .From(Tables.Users)
                .Where(Tables.Users.ColUsername == user.Username)
                .ReadValue(Tables.Users.ColId);

            if (existingUserId.HasValue)
                throw new ApplicationException("Dieser Benutzername wurde bereits vergeben");

            // generate a password
            var password = GeneratePassword();
            var pwHash = CryptHash(password);

            // insert
            _database.Insert
                .In(Tables.Users)
                .Set(Tables.Users.ColUsername.To(user.Username),
                     Tables.Users.ColFirstName.To(user.FirstName),
                     Tables.Users.ColLastName.To(user.LastName),
                     Tables.Users.ColRole.To(user.Role),
                     Tables.Users.ColIsActive.To(true),
                     Tables.Users.ColPassword.To(pwHash))
                .Execute();

            // TODO: Send email
        }

        public void Delete(int id)
        {
            var otherAdminId = _database.Select
                .From(Tables.Users)
                .Where(Tables.Users.ColId != id & Tables.Users.ColIsActive == true & Tables.Users.ColRole == "Admin")
                .ReadValue(Tables.Users.ColId);

            if (!otherAdminId.HasValue)
                throw new ApplicationException("Sie können den letzen Benutzer mit der Rolle Admin nicht löschen");

            var deletedUser = _database.Update
                .In(Tables.Users)
                .Where(Tables.Users.ColId == id)
                .Set(Tables.Users.ColIsActive.To(false))
                .Execute();

            if (deletedUser == 0)
            {
                throw new ApplicationException("Der Benutzer wurde durch einen Benutzer gelöscht");
            }
        }

        public void Update(UserChanges userChanges)
        {
            if (userChanges.Username != userChanges.OrigUsername)
            {
                var otherUser = _database.Select
                    .From(Tables.Users)
                    .Where(Tables.Users.ColUsername == userChanges.Username & Tables.Users.ColIsActive == true)
                    .ReadValue(Tables.Users.ColId);

                if (otherUser.HasValue)
                    throw new ApplicationException("Dieser Benutzername wurde bereits vergeben");
            }

            if (userChanges.Role != "Admin" && userChanges.OrigRole == "Admin")
            {
                var otherAdmin = _database.Select
                    .From(Tables.Users)
                    .Where(Tables.Users.ColId != userChanges.Id & Tables.Users.ColIsActive == true & Tables.Users.ColRole == "Admin")
                    .ReadValue(Tables.Users.ColId);

                if (!otherAdmin.HasValue)
                    throw new ApplicationException("Mindestens ein Benutzer benötigt die Rolle Admin. Eine Änderung ist nicht möglich");
            }

            var updatedUser = _database.Update
                .In(Tables.Users)
                .Where(Tables.Users.ColId == userChanges.Id)
                .Set(Tables.Users.ColUsername.To(userChanges.Username),
                    Tables.Users.ColFirstName.To(userChanges.FirstName),
                    Tables.Users.ColLastName.To(userChanges.LastName),
                    Tables.Users.ColRole.To(userChanges.Role))
                .Execute();

            if (updatedUser == 0)
            {
                throw new ApplicationException("Der Benutzer wurde durch einen anderen Benutzer gelöscht");
            }
        }

        public void ChangePassword(ChangeUserPassword user)
        {
            var password = SqlBuilder.Select
                .From(Tables.Users)
                .Where(Tables.Users.ColId == user.Id)
                .ReadNullableValue(_database, Tables.Users.ColPassword);

            if (password == null)
                throw new ApplicationException("Der angemeldete Benutzer konnte nicht gefunden werden");

            // check password/hash
            if (!CryptVerify(user.Password, password))
                throw new ApplicationException("Das eingegebenen Kennwort ist ungültig");

            _database.Update.In(Tables.Users).Where(Tables.Users.ColId == user.Id)
                .Set(Tables.Users.ColIsActive.To(true))
                .Set(Tables.Users.ColPassword.To(CryptHash(user.NewPassword)))
                .Execute();
        }

        public void ResetPassword(ResetPasswordRequest request)
        {
            if (request == null)
                return;

            var query = _database.Select
                .From(Tables.Users)
                .Where(Tables.Users.ColUsername == request.Username & Tables.Users.ColIsActive == true);

            var user = query.ReadAll(User.Mapper).SingleOrDefault();

            if (user == null)
                throw new ApplicationException("Der eingegebene Benutzername existiert nicht");

            // generate a new password
            var password = GeneratePassword();
            var pwHash =CryptHash(password);

            _database.Update.In(Tables.Users).Where(Tables.Users.ColId == user.Id)
                .Set(Tables.Users.ColPassword.To(pwHash))
                .Execute();

            // TODO: Send email
        }

        // helper methods
        private string generateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private RefreshToken generateRefreshToken(string ipAddress)
        {
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow,
                    CreatedByIp = ipAddress
                };
            }
        }

        private string GeneratePassword()
        {
            return "ThisIsARandom!Password";
        }

        private string CryptHash(string password)
        {
            return string.Empty;
        }

        private bool CryptVerify(string password1, string password2)
        {
            return true;
        }
    }
}