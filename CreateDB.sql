DROP DATABASE IF EXISTS ObserverDB;

CREATE DATABASE ObserverDB
DEFAULT CHARACTER SET utf8
DEFAULT COLLATE utf8_general_ci;

USE ObserverDB;

CREATE USER 'observerUSER'@'localhost' IDENTIFIED BY 'observerPWD';
GRANT ALL PRIVILEGES ON observerdb.* TO 'observerUSER'@'localhost';

CREATE TABLE Infoes
(
	Id 				INT AUTO_INCREMENT,
    Sender			VARCHAR (100),
    SenderEmail		VARCHAR (100),
    SenderPwd 		VARCHAR (100),
    Receiver		VARCHAR (100),
    ReceiverEmail	VARCHAR (100),
    HostPort		INT,
    HostName		VARCHAR (100),
    SslEnable		BOOL,

	CONSTRAINT	info_pk
    PRIMARY KEY (Id)
);


CREATE TABLE Users
(
	UserId		INT AUTO_INCREMENT,
    Rights		INT,
    Login		VARCHAR(50),
	
    CONSTRAINT users_pk
    PRIMARY KEY (UserId)
);

CREATE TABLE Hashes
(
	HashId		INT AUTO_INCREMENT,
	UserId		INT,
    UserHash	VARCHAR(64),
	
    CONSTRAINT	hashes_pk
    PRIMARY KEY (HashId),
    
    CONSTRAINT	hashes_fk
    FOREIGN KEY (UserId)
    REFERENCES	Users(UserId)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);

CREATE TABLE EventTypes
(
	EventId		INT AUTO_INCREMENT,
    Type		TEXT,

	CONSTRAINT	event_pk 
    PRIMARY KEY	(EventId)
);

CREATE TABLE EventLogs
(
	Id			INT AUTO_INCREMENT,
	EventId		INT,
    UserId		INT,
    Coords		VARCHAR(11),
    TimeCode	DATETIME,

    CONSTRAINT eventlog_pk
    PRIMARY KEY (Id),
    
    CONSTRAINT eventlog_user_fk 
    FOREIGN KEY (UserId)
    REFERENCES Users(UserId)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
    
    CONSTRAINT eventlog_event_fk
    FOREIGN KEY (EventId) 
    REFERENCES EventTypes(EventId)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);

/* Add administrator */
INSERT INTO Users (Rights, Login)
VALUES
	(1, "Admin");

INSERT INTO Hashes (UserId, UserHash)
VALUES (
    (
		SELECT UserId FROM Users WHERE Login = "Admin"),
		"c1c224b03cd9bc7b6a86d77f5dace40191766c485cd55dc48caf9ac873335d6f"
    );
    
INSERT INTO Infoes (Sender,
					SenderEmail,
                    SenderPwd,
                    Receiver,
                    ReceiverEmail,
                    HostPort,
                    HostName,
                    SslEnable
                    )
VALUES
	("Observer notifier",
    "expertconcordance@gmail.com",
    "mdfkkfeorujxohby",
    "Observer listener",
    "timergaliev.work@gmail.com",
    465,
    "smtp.gmail.com",
    true
    ); 

INSERT INTO EventTypes (Type)
VALUES
	("Left button clicked"),
    ("Right button clicked"),
    ("Middle button clicked"),
    ("Mouse moved");

/* We do not need to add events */
/*
INSERT INTO EventLogs (EventId, UserId, Coords, TimeCode)
VALUES
	(1, 1, "10;10", "2021-07-02 04:48:41"),
    (3, 1, "100;100", "2021-07-02 04:48:45");
*/
