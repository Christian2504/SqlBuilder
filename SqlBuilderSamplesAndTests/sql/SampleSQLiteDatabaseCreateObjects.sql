/*
--------------------------------------------------------------------
© 2017 sqlservertutorial.net All Rights Reserved
--------------------------------------------------------------------
Name   : BikeStores
Link   : http://www.sqlservertutorial.net/load-sample-database/
Version: 1.0
--------------------------------------------------------------------
*/

-- create tables

CREATE TABLE Users
(
  Id                INTEGER       NOT NULL PRIMARY KEY AUTOINCREMENT,
  IsActive          BOOLEAN       NOT NULL,
  FirstName         VARCHAR(60)   NOT NULL,
  LastName          VARCHAR(60)   NOT NULL,
  Username          VARCHAR(25)   NOT NULL,
  Password          CHAR(60)      NOT NULL,
  Role              VARCHAR(20)   NOT NULL,
  CONSTRAINT UNIQUE_Users_Username UNIQUE (Username)
);

CREATE TABLE RefreshToken
(
  Token           VARCHAR(100)      NOT NULL PRIMARY KEY,
  UserId          INTGER            NOT NULL,
  Expires         DATETIME          NOT NULL,
  Created         DATETIME          NOT NULL,
  CreatedByIp     VARCHAR(40)       NOT NULL,
  Revoked         DATETIME                  ,
  RevokedByIp     VARCHAR(40)               ,
  ReplacedByToken VARCHAR(100)              ,
  CONSTRAINT FK_RefreshToken_Users FOREIGN KEY (UserId)
  REFERENCES Users (Id)
  ON DELETE CASCADE
  ON UPDATE CASCADE
);

CREATE TABLE categories (
	category_id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	category_name VARCHAR (255) NOT NULL
);

CREATE TABLE brands (
	brand_id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	brand_name VARCHAR (255) NOT NULL
);

CREATE TABLE products (
	product_id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	product_name VARCHAR (255) NOT NULL,
	brand_id INT NOT NULL,
	category_id INT NOT NULL,
	model_year SMALLINT NOT NULL,
	list_price DECIMAL (10, 2) NOT NULL,
	FOREIGN KEY (category_id) REFERENCES categories (category_id) ON DELETE CASCADE ON UPDATE CASCADE,
	FOREIGN KEY (brand_id) REFERENCES brands (brand_id) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE customers (
	customer_id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	first_name VARCHAR (255) NOT NULL,
	last_name VARCHAR (255) NOT NULL,
	phone VARCHAR (25),
	email VARCHAR (255) NOT NULL,
	street VARCHAR (255),
	city VARCHAR (50),
	state VARCHAR (25),
	zip_code VARCHAR (5)
);

CREATE TABLE stores (
	store_id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	store_name VARCHAR (255) NOT NULL,
	phone VARCHAR (25),
	email VARCHAR (255),
	street VARCHAR (255),
	city VARCHAR (255),
	state VARCHAR (10),
	zip_code VARCHAR (5)
);

CREATE TABLE staffs (
	staff_id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	first_name VARCHAR (50) NOT NULL,
	last_name VARCHAR (50) NOT NULL,
	email VARCHAR (255) NOT NULL UNIQUE,
	phone VARCHAR (25),
	active tinyint NOT NULL,
	store_id INT NOT NULL,
	manager_id INT,
	FOREIGN KEY (store_id) REFERENCES stores (store_id) ON DELETE CASCADE ON UPDATE CASCADE,
	FOREIGN KEY (manager_id) REFERENCES staffs (staff_id) ON DELETE NO ACTION ON UPDATE NO ACTION
);

CREATE TABLE orders (
	order_id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	customer_id INT,
	order_status tinyint NOT NULL,
	-- Order status: 1 = Pending; 2 = Processing; 3 = Rejected; 4 = Completed
	order_date DATE NOT NULL,
	required_date DATE NOT NULL,
	shipped_date DATE,
	store_id INT NOT NULL,
	staff_id INT NOT NULL,
	FOREIGN KEY (customer_id) REFERENCES customers (customer_id) ON DELETE CASCADE ON UPDATE CASCADE,
	FOREIGN KEY (store_id) REFERENCES stores (store_id) ON DELETE CASCADE ON UPDATE CASCADE,
	FOREIGN KEY (staff_id) REFERENCES staffs (staff_id) ON DELETE NO ACTION ON UPDATE NO ACTION
);

CREATE TABLE order_items (
	order_id INT,
	item_id INT,
	product_id INT NOT NULL,
	quantity INT NOT NULL,
	list_price DECIMAL (10, 2) NOT NULL,
	discount DECIMAL (4, 2) NOT NULL DEFAULT 0,
	PRIMARY KEY (order_id, item_id),
	FOREIGN KEY (order_id) REFERENCES orders (order_id) ON DELETE CASCADE ON UPDATE CASCADE,
	FOREIGN KEY (product_id) REFERENCES products (product_id) ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE TABLE stocks (
	store_id INT,
	product_id INT,
	quantity INT,
	PRIMARY KEY (store_id, product_id),
	FOREIGN KEY (store_id) REFERENCES stores (store_id) ON DELETE CASCADE ON UPDATE CASCADE,
	FOREIGN KEY (product_id) REFERENCES products (product_id) ON DELETE CASCADE ON UPDATE CASCADE
);