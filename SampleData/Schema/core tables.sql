CREATE TABLE `customer__customer` (
  `id` char(32) NOT NULL,
  `firstName` varchar(100) NOT NULL,
  `middleName` varchar(100) DEFAULT NULL,
  `lastName` varchar(100) NOT NULL,
  `displayName` varchar(200) NOT NULL,
  `contact` varchar(50) DEFAULT NULL,
  `email` varchar(200) DEFAULT NULL,
  `dob` datetime DEFAULT NULL,
  PRIMARY KEY (`id`)
  --,KEY md5(concat('firstName','middleName','lastName')) (`firstName`,`middleName`,`lastName`)
  --,KEY md5(concat('contact')) (`contact`)
  --,KEY md5(concat('displayName')) (`displayName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT = 'V:20170808';

CREATE TABLE `membership__membership` (
  `id` char(32) NOT NULL,
  `name` varchar(50) DEFAULT NULL,
  `description` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`id`)
  --,KEY md5(concat('name')) (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `customer__membership` (
  `id` char(32) NOT NULL,
  `customerId` varchar(32) DEFAULT NULL,
  `membershipId` varchar(32) DEFAULT NULL,
  `date_start datetime DEFAULT NULL,
  `date_end` datetime DEFAULT NULL,
  PRIMARY KEY (`id`)
  --,KEY md5(concat('customerId')) (`membershipId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `field__field` (
  `id` char(32) NOT NULL,
  `name` varchar(100) DEFAULT NULL,
  `description` varchar(250) DEFAULT NULL,
  `fieldType` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`id`)
  --,KEY  md5(concat('fieldType','name'))(`fieldType`,`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `field__fieldConfig` (
  `id` char(32) NOT NULL,
  `name` varchar(100) DEFAULT NULL,
  `description` varchar(250) DEFAULT NULL,
  `fieldType` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`id`)
  --,KEY  md5(concat('fieldType','name'))(`fieldType`,`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `customer__customerField` (
  `id` char(32) NOT NULL,
  `name` varchar(100) DEFAULT NULL,
  `description` varchar(250) DEFAULT NULL,
  `fieldType` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`id`)
  --,KEY  md5(concat('fieldType','name'))(`fieldType`,`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `customer__fieldInt` (
  `id` char(32) NOT NULL,
  `customerId` char(32) DEFAULT NULL,
  `custFieldId` char(32) DEFAULT NULL,
  `value` int,
  PRIMARY KEY (`id`)
  --,KEY `b964c81367ba98ed455b3a25057be1c0` (`custFieldId`,`customerId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `customer__fieldVarchar` (
  `id` char(32) NOT NULL,
  `customerId` char(32) DEFAULT NULL,
  `custFieldId` char(32) DEFAULT NULL,
  `value` varchar(360),
  PRIMARY KEY (`id`)
  --,KEY `b964c81367ba98ed455b3a25057be1c0` (`custFieldId`,`customerId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `customer__fieldDouble` (
  `id` char(32) NOT NULL,
  `customerId` char(32) DEFAULT NULL,
  `custFieldId` char(32) DEFAULT NULL,
  `value` double,
  PRIMARY KEY (`id`)
  --,KEY `b964c81367ba98ed455b3a25057be1c0` (`custFieldId`,`customerId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `customer__fieldDatetime` (
  `id` char(32) NOT NULL,
  `customerId` char(32) DEFAULT NULL,
  `custFieldId` char(32) DEFAULT NULL,
  `value` datetime,
  PRIMARY KEY (`id`)
  --,KEY `b964c81367ba98ed455b3a25057be1c0` (`custFieldId`,`customerId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `customer__fieldBoolean (
  `id` char(32) NOT NULL,x
  `customerId` char(32) DEFAULT NULL,
  `custFieldId` char(32) DEFAULT NULL,
  `value` boolean,
  PRIMARY KEY (`id`)
  --,KEY `b964c81367ba98ed455b3a25057be1c0` (`custFieldId`,`customerId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

