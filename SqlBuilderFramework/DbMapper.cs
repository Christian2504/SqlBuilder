using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlBuilderFramework
{
    public interface IDbMapperEntry<C>
    {
        void BindColumns(SqlBuilder sqlBuilder);
        bool IsNull { get; }
        void SetValues(C entity);
    }

    public class DbMapperEntry<C, T> : IDbMapperEntry<C>
    {
        private readonly DbTypedColumn<T> column;
        private DbTypedBoundValue<T> boundValue;
        private readonly Action<C, T> setter;

        public DbMapperEntry(DbTypedColumn<T> column, Action<C, T> setter)
        {
            this.column = column;
            this.setter = setter;
        }

        public void BindColumns(SqlBuilder sqlBuilder)
        {
            boundValue = sqlBuilder.Bind(column);
        }

        public bool IsNull => boundValue.IsNull;

        public void SetValues(C entity)
        {
            setter(entity, boundValue.Value);
        }
    }

    public class DbMapperEntry<C, T1, T2> : IDbMapperEntry<C>
    {
        private readonly DbTypedColumn<T1> column1;
        private DbTypedBoundValue<T1> boundValue1;
        private readonly DbTypedColumn<T2> column2;
        private DbTypedBoundValue<T2> boundValue2;
        private readonly Action<C, T1, T2> setter;

        public DbMapperEntry(DbTypedColumn<T1> column1, DbTypedColumn<T2> column2, Action<C, T1, T2> setter)
        {
            this.column1 = column1;
            this.column2 = column2;
            this.setter = setter;
        }

        public void BindColumns(SqlBuilder sqlBuilder)
        {
            boundValue1 = sqlBuilder.Bind(column1);
            boundValue2 = sqlBuilder.Bind(column2);
        }

        public bool IsNull => boundValue1.IsNull && boundValue2.IsNull;

        public void SetValues(C entity)
        {
            setter(entity, boundValue1.Value, boundValue2.Value);
        }
    }

    public class DbMapperEntry<C, T1, T2, T3> : IDbMapperEntry<C>
    {
        private readonly DbTypedColumn<T1> column1;
        private DbTypedBoundValue<T1> boundValue1;
        private readonly DbTypedColumn<T2> column2;
        private DbTypedBoundValue<T2> boundValue2;
        private readonly DbTypedColumn<T3> column3;
        private DbTypedBoundValue<T3> boundValue3;
        private readonly Action<C, T1, T2, T3> setter;

        public DbMapperEntry(DbTypedColumn<T1> column1, DbTypedColumn<T2> column2, DbTypedColumn<T3> column3, Action<C, T1, T2, T3> setter)
        {
            this.column1 = column1;
            this.column2 = column2;
            this.column3 = column3;
            this.setter = setter;
        }

        public void BindColumns(SqlBuilder sqlBuilder)
        {
            boundValue1 = sqlBuilder.Bind(column1);
            boundValue2 = sqlBuilder.Bind(column2);
            boundValue3 = sqlBuilder.Bind(column3);
        }

        public bool IsNull => boundValue1.IsNull && boundValue2.IsNull && boundValue3.IsNull;

        public void SetValues(C entity)
        {
            setter(entity, boundValue1.Value, boundValue2.Value, boundValue3.Value);
        }
    }

    public class DbMapperEntry<C, T1, T2, T3, T4> : IDbMapperEntry<C>
    {
        private readonly DbTypedColumn<T1> column1;
        private DbTypedBoundValue<T1> boundValue1;
        private readonly DbTypedColumn<T2> column2;
        private DbTypedBoundValue<T2> boundValue2;
        private readonly DbTypedColumn<T3> column3;
        private DbTypedBoundValue<T3> boundValue3;
        private readonly DbTypedColumn<T4> column4;
        private DbTypedBoundValue<T4> boundValue4;
        private readonly Action<C, T1, T2, T3, T4> setter;

        public DbMapperEntry(DbTypedColumn<T1> column1, DbTypedColumn<T2> column2, DbTypedColumn<T3> column3, DbTypedColumn<T4> column4, Action<C, T1, T2, T3, T4> setter)
        {
            this.column1 = column1;
            this.column2 = column2;
            this.column3 = column3;
            this.column4 = column4;
            this.setter = setter;
        }

        public void BindColumns(SqlBuilder sqlBuilder)
        {
            boundValue1 = sqlBuilder.Bind(column1);
            boundValue2 = sqlBuilder.Bind(column2);
            boundValue3 = sqlBuilder.Bind(column3);
            boundValue4 = sqlBuilder.Bind(column4);
        }

        public bool IsNull => boundValue1.IsNull && boundValue2.IsNull && boundValue3.IsNull && boundValue4.IsNull;

        public void SetValues(C entity)
        {
            setter(entity, boundValue1.Value, boundValue2.Value, boundValue3.Value, boundValue4.Value);
        }
    }

    public interface IGroupMapper<C>
    {
        void BindColumns(SqlBuilder sqlBuilder);

        void NewList();

        void SetList(C entity);

        void AddEntity();
    }

    public class GroupMapper<C, E> : IGroupMapper<C>, IDbMapperEntry<C> where E : new()
    {
        private readonly DbMapper<E> mapper;
        private readonly Action<C, List<E>> setter;

        public GroupMapper(DbMapper<E> mapper, Action<C, List<E>> setter)
        {
            this.mapper = mapper;
            this.setter = setter;
        }
        public void BindColumns(SqlBuilder sqlBuilder)
        {
            mapper.BindColumns(sqlBuilder);
        }

        public bool IsNull => mapper.IsNull;

        public void SetValues(C entity)
        {
            if (mapper.IsNull)
                return;

            mapper.NewList();
            mapper.AddEntity();
            mapper.End();
            setter(entity, mapper.EntityList);
        }

        public void NewList()
        {
            mapper.NewList();
        }

        public void SetList(C entity)
        {
            setter(entity, mapper.EntityList);
        }

        public void AddEntity()
        {
            mapper.AddEntity();
        }
    }

    public interface IGroupPredicate
    {
        void BindColumns(SqlBuilder sqlBuilder);

        bool GroupChanged();
    }

    public class GroupPredicate<T> : IGroupPredicate
    {
        private readonly DbTypedColumn<T> column;
        private DbTypedBoundValue<T> boundValue;
        private T value;
        private readonly Func<T, T, bool> predicate;

        public GroupPredicate(DbTypedColumn<T> column, Func<T, T, bool> predicate)
        {
            this.column = column;
            this.predicate = predicate;
        }

        public void BindColumns(SqlBuilder sqlBuilder)
        {
            boundValue = sqlBuilder.Bind(column);
        }

        public bool GroupChanged()
        {
            var result = predicate(value, boundValue.Value);

            value = boundValue.Value;

            return result;
        }
    }

    public class DbMapper<C> where C : new()
    {
        private readonly Func<C> newEntity;

        private List<IDbMapperEntry<C>> entries = new List<IDbMapperEntry<C>>();

        private IGroupMapper<C> groupMapper;

        private IGroupPredicate explicitGroupPredicate;

        private Func<C, C, bool> groupPredicate;

        public List<C> EntityList { get; private set; }

        public DbMapper()
        {
        }

        public DbMapper(Func<C> newEntity)
        {
            this.newEntity = newEntity;
        }

        public DbMapper<C> Map<T>(DbTypedColumn<T> column, Action<C, T> setter)
        {
            entries.Add(new DbMapperEntry<C, T>(column, setter));
            return this;
        }

        public DbMapper<C> Map<T1, T2>(DbTypedColumn<T1> column1, DbTypedColumn<T2> column2, Action<C, T1, T2> setter)
        {
            entries.Add(new DbMapperEntry<C, T1, T2>(column1, column2, setter));
            return this;
        }

        public DbMapper<C> Map<T1, T2, T3>(DbTypedColumn<T1> column1, DbTypedColumn<T2> column2, DbTypedColumn<T3> column3, Action<C, T1, T2, T3> setter)
        {
            entries.Add(new DbMapperEntry<C, T1, T2, T3>(column1, column2, column3, setter));
            return this;
        }

        public DbMapper<C> Map<T1, T2, T3, T4>(DbTypedColumn<T1> column1, DbTypedColumn<T2> column2, DbTypedColumn<T3> column3, DbTypedColumn<T4> column4, Action<C, T1, T2, T3, T4> setter)
        {
            entries.Add(new DbMapperEntry<C, T1, T2, T3, T4>(column1, column2, column3, column4, setter));
            return this;
        }

        public DbMapper<C> Map<E>(DbMapper<E> mapper, Action<C, List<E>> setter) where E : new()
        {
            entries.Add(new GroupMapper<C, E>(mapper, setter));
            return this;
        }

        public DbMapper<C> Group<E>(DbMapper<E> mapper, Action<C, List<E>> setter) where E : new()
        {
            groupMapper = new GroupMapper<C, E>(mapper, setter);
            return this;
        }

        public DbMapper<C> GroupWhen<T>(DbTypedColumn<T> column, Func<T, T, bool> predicate)
        {
            explicitGroupPredicate = new GroupPredicate<T>(column, predicate);
            return this;
        }

        public DbMapper<C> GroupWhen(Func<C, C, bool> predicate)
        {
            groupPredicate = predicate;
            return this;
        }

        public void BindColumns(SqlBuilder sqlBuilder)
        {
            foreach (var entry in entries)
            {
                entry.BindColumns(sqlBuilder);
            }

            groupMapper?.BindColumns(sqlBuilder);
            explicitGroupPredicate?.BindColumns(sqlBuilder);
        }

        public bool IsNull => entries.All(e => e.IsNull);

        public void NewList()
        {
            EntityList = new List<C>();
        }

        public void AddEntity()
        {
            if (groupMapper != null)
            {
                if (groupPredicate != null)
                {
                    var entity = NewEntity();

                    if (groupPredicate(EntityList.LastOrDefault(), entity))
                    {
                        if (EntityList.Any())
                        {
                            groupMapper.SetList(EntityList.Last());
                        }

                        EntityList.Add(entity);

                        groupMapper.NewList();
                    }
                }
                else if (explicitGroupPredicate == null || explicitGroupPredicate.GroupChanged() || !EntityList.Any())
                {
                    if (EntityList.Any())
                    {
                        groupMapper.SetList(EntityList.Last());
                    }

                    var entity = NewEntity();

                    EntityList.Add(entity);

                    groupMapper.NewList();
                }

                groupMapper.AddEntity();
            }
            else if (explicitGroupPredicate == null || explicitGroupPredicate.GroupChanged() || !EntityList.Any())
            {
                EntityList.Add(NewEntity());
            }
            else
            {
                var entity = EntityList.Last();

                foreach (var entry in entries)
                {
                    entry.SetValues(entity);
                }
            }
        }

        public void End()
        {
            if (groupMapper != null && EntityList.Any())
            {
                groupMapper.SetList(EntityList.Last());
            }
        }

        private C NewEntity()
        {
            var entity = newEntity == null ? new C() : newEntity();

            foreach (var entry in entries)
            {
                entry.SetValues(entity);
            }

            return entity;
        }
    }
}
