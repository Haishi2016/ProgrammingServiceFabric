CREATE TABLE IF NOT EXISTS pet (
	name varchar(20) NOT NULL,
	owner varchar(20) NOT NULL,
	species varchar(20) NOT NULL,
	sex char(1) NOT NULL,
	birth TIMESTAMP WITH TIME ZONE NOT NULL,
	death TIMESTAMP WITH TIME ZONE);
 
INSERT INTO pet VALUES('Puffball', 'Diane', 'hamster', 'f', '1999-03-30', NULL);
INSERT INTO pet VALUES('Leroy', 'Sabrina', 'cat', 'm', '1997-05-04', NULL);
INSERT INTO pet VALUES('Naoao', 'Haiying', 'dog', 'm', '1993-01-23', '2001-09-14');
