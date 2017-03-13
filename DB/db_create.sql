CREATE SCHEMA IF NOT EXISTS `tlenbot` ;

CREATE TABLE IF NOT EXISTS `tlenbot`.`participants` (
  `Id` INT NOT NULL,
  `UserName` VARCHAR(32) NULL,
  `FirstName` TEXT NULL,
  `LastName` TEXT NULL,
  PRIMARY KEY (`Id`),
  INDEX `IND_name` (`UserName`));

CREATE TABLE IF NOT EXISTS `tlenbot`.`messages` (
  `ChatId` BIGINT(8) NOT NULL,
  `ParticipantId` INT NOT NULL,
  `Date` DATE NOT NULL,
  `MessageType` INT NOT NULL,
  `Counter` INT NULL DEFAULT 1,
  INDEX `FK_participants_idx` (`ParticipantId` ASC),
  INDEX `IND_chatId_date_` (`ChatId` ASC, `Date` ASC),
  CONSTRAINT `FK_participants`
    FOREIGN KEY (`ParticipantId`)
    REFERENCES `tlenbot`.`participants` (`Id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
CONSTRAINT `UN_data` UNIQUE (`ChatId`,`ParticipantId`,`Date`,`MessageType`));

