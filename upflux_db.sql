/*CREATE DATABASE upflux*/

USE upflux;

/*Create Table Statements*/

-- Create Machines Table
CREATE TABLE Machines (
    machine_id VARCHAR(255) PRIMARY KEY,
    date_added TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ip_address VARCHAR(15) NOT NULL ,
    machine_name varchar(255) NOT NULL
);

-- Create Users Table
CREATE TABLE Users (
    user_id VARCHAR(50) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    role ENUM('Admin', 'Engineer') NOT NULL,
	last_login TIMESTAMP
);

CREATE TABLE Revoked_Tokens (
    revoke_id INT PRIMARY KEY AUTO_INCREMENT,
    user_id VARCHAR(50),
    revoked_by VARCHAR(50) NOT NULL,
    revoked_at DATETIME NOT NULL,
    reason MEDIUMTEXT,
    CONSTRAINT fk_user FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE,
    KEY (user_id),
    KEY (revoked_by)
);


-- Create Admin_Details Table 
CREATE TABLE Admin_Details (
    admin_id VARCHAR(50) PRIMARY KEY,
    user_id VARCHAR(50) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    FOREIGN KEY (user_id) REFERENCES Users(user_id)
);

-- Create Licences Table
CREATE TABLE Licences (
    licence_key VARCHAR(255) NOT NULL PRIMARY KEY,
    machine_id VARCHAR(255) NOT NULL,
    expiration_date TIMESTAMP NOT NULL,
    FOREIGN KEY (machine_id) REFERENCES Machines(machine_id)
);

-- Create Credentials Table
CREATE TABLE Credentials (
    credential_id INT AUTO_INCREMENT PRIMARY KEY, 
    user_id VARCHAR(50) NOT NULL,                  
    machine_id VARCHAR(255) NOT NULL,                 
    access_granted_at TIMESTAMP NOT NULL,   
    expires_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,           
    access_granted_by VARCHAR(50) NOT NULL,  
    FOREIGN KEY (user_id) REFERENCES Users(user_id),
    FOREIGN KEY (machine_id) REFERENCES Machines(machine_id),
    FOREIGN KEY (access_granted_by) REFERENCES Admin_Details(admin_id),
    UNIQUE (user_id, machine_id, credential_id)   
);


-- Create Packages Table
CREATE TABLE Packages (
    package_id INT AUTO_INCREMENT PRIMARY KEY,
    version_number FLOAT NOT NULL,
    package_size FLOAT NOT NULL,
    package_signature VARCHAR(255) NOT NULL,
    release_date TIMESTAMP NOT NULL
);

-- Create Update Logs Table
CREATE TABLE Update_Logs (
    update_id INT AUTO_INCREMENT PRIMARY KEY,
    package_id INT NOT NULL,
    user_id VARCHAR(50) NOT NULL,
    machine_id VARCHAR(255) NOT NULL,
    update_status ENUM('Pending', 'Completed', 'Failed') NOT NULL,
    time_applied TIMESTAMP NOT NULL,
    FOREIGN KEY (package_id) REFERENCES Packages(package_id),
    FOREIGN KEY (user_id) REFERENCES Users(user_id),
    FOREIGN KEY (machine_id) REFERENCES Machines(machine_id)
);

-- Create Action Logs Table
CREATE TABLE Action_Logs (
    log_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id VARCHAR(50) NOT NULL,
    action_type ENUM('CREATE', 'UPDATE', 'DELETE') NOT NULL,
	entity_name VARCHAR(255) NOT NULL,
    time_performed TIMESTAMP NOT NULL,
    FOREIGN KEY (user_id) REFERENCES Users(user_id)
);

-- Create Applications Table
CREATE TABLE Applications (
    app_id INT AUTO_INCREMENT PRIMARY KEY,
	machine_id VARCHAR(255) NOT NULL,
    app_name VARCHAR(255) NOT NULL,
    added_by VARCHAR(50) NOT NULL,
    current_version VARCHAR(50) NOT NULL,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (added_by) REFERENCES Users(user_id),
    FOREIGN KEY (machine_id) REFERENCES Machines(machine_id) 
);

-- Create Application_Versions Table
CREATE TABLE Application_Versions (
    version_id INT AUTO_INCREMENT PRIMARY KEY,  
    machine_id VARCHAR(255) NOT NULL,
    version_name VARCHAR(50) NOT NULL, 
    date TIMESTAMP NOT NULL,
    FOREIGN KEY (machine_id) REFERENCES Machines(machine_id)
);

CREATE TABLE Generated_Machine_Ids (
    generated_uuid VARCHAR(36) PRIMARY KEY, 
    machine_id VARCHAR(255) NOT NULL,  
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (machine_id) REFERENCES Machines(machine_id) ON DELETE CASCADE
);

CREATE TABLE Machine_Status (
    machine_id PRIMARY KEY,  
    isOnline BOOLEAN,
    lastSeen TIMESTAMP,
    FOREIGN KEY (machine) REFERENCES Machines(machine_id) ON DELETE CASCADE
);

-- Temporary table for tracking user id in a session
CREATE TEMPORARY TABLE User_Context (
    session_id CHAR(36) NOT NULL DEFAULT (UUID()), 
    user_id VARCHAR(50) NOT NULL,               
    PRIMARY KEY (session_id)
);


/*Show all tables present in the database*/
SHOW TABLES;

-- Insert into Machines
INSERT INTO Machines (machine_id, ip_address, machine_name) VALUES
('MCH123ABC', '192.168.0.1', 'M456'),
('MCH456DEF', '192.168.0.2', 'M789'),
('MCH789GHI', '192.168.0.3', 'M321');

-- Insert into Users
INSERT INTO Users (user_id, name, email, role, last_login) VALUES
('E123', 'Alice Johnson', 'alice@example.com', 'Engineer', '2024-01-15 08:00:00'),
('E456', 'Bob Smith', 'bob@example.com', 'Admin', '2024-01-16 09:30:00'),
('E789', 'Charlie Davis', 'charlie@example.com', 'Engineer', '2024-01-17 10:15:00');

-- Insert into Admin_Details
INSERT INTO Admin_Details (admin_id, user_id, password_hash) VALUES
('A123', 'E456', 'hash_password_1'),
('A456', 'E456', 'hash_password_2'),
('A789', 'E456', 'hash_password_3');

-- Insert into Licences
INSERT INTO Licences (licence_key, machine_id, expiration_date) VALUES
('LIC123ABC', 'MCH123ABC', '2025-01-15 00:00:00'),
('LIC456DEF', 'MCH456DEF', '2025-02-20 00:00:00'),
('LIC789GHI', 'MCH789GHI', '2025-03-25 00:00:00');

-- Insert into Credentials
INSERT INTO Credentials (user_id, machine_id, access_granted_at, expires_at, access_granted_by) VALUES
('E123', 'MCH123ABC', '2024-01-15 08:00:00', '2025-01-15 08:00:00', 'A123'),
('E456', 'MCH456DEF', '2024-01-16 09:30:00', '2025-01-16 09:30:00', 'A456'),
('E789', 'MCH789GHI', '2024-01-17 10:15:00', '2025-01-17 10:15:00', 'A789');

-- Insert into Packages
INSERT INTO Packages (version_number, package_size, package_signature, release_date) VALUES
(1.0, 150.5, 'sig_abc123', '2024-01-10 00:00:00'),
(1.1, 200.0, 'sig_def456', '2024-01-20 00:00:00'),
(1.2, 250.75, 'sig_ghi789', '2024-01-30 00:00:00');

-- Insert into Update_Logs
INSERT INTO Update_Logs (package_id, user_id, machine_id, update_status, time_applied) VALUES
(1, 'E123', 'MCH123ABC', 'Completed', '2024-01-15 08:30:00'),
(2, 'E456', 'MCH456DEF', 'Pending', '2024-01-16 10:00:00'),
(3, 'E789', 'MCH789GHI', 'Failed', '2024-01-17 11:00:00');

-- Insert into Action_Logs
INSERT INTO Action_Logs (user_id, action_type, entity_name, time_performed) VALUES
('E123', 'CREATE', 'Machine', '2024-01-15 08:45:00'),
('E456', 'UPDATE', 'Package', '2024-01-16 10:15:00'),
('E789', 'DELETE', 'Licence', '2024-01-17 11:30:00');

-- Insert into Applications
INSERT INTO Applications (machine_id, app_name, added_by, current_version) VALUES
('MCH123ABC', 'AppOne', 'E123', 'v1.0'),
('MCH456DEF', 'AppTwo', 'E456', 'v1.1'),
('MCH789GHI', 'AppThree', 'E789', 'v1.2');

-- Insert into Application_Versions
INSERT INTO Application_Versions (machine_id, version_name,  date) VALUES
('MCH123ABC', 'v1.0.1', '2024-01-15 09:00:00'),
('MCH123ABC', 'v1.1.1', '2024-01-16 10:30:00'),
('MCH123ABC', 'v1.2.1', '2024-01-17 11:45:00');

-- Insert into Generated_Machine_Ids
INSERT INTO Generated_Machine_Ids (machine_id) VALUES
('123e4567-e89b-12d3-a456-426614174000'),
('987f6543-e21b-34c2-b789-526613274111'),
('456a1234-b56c-45f1-c321-626612374222');

-- View Action Logs table data
SELECT * FROM Action_Logs;

/*Basic Queries*/

/*Show all users*/
SELECT * FROM Machines;

/*Show all users*/
SELECT * FROM Users;

/*Show all packages*/
SELECT * FROM Packages;

/*Show all licences*/
SELECT * FROM Licences

/*Update-Related Queries*'/

/*Show all machine current versions*/
SELECT 
	m.machine_id, p.version_number
FROM
	Machines m
JOIN 
	Update_Logs ul ON m.machine_id = ul.machine_id
JOIN 
	Packages p ON ul.package_id = p.package_id
WHERE 
	ul.update_status = 'Completed'
ORDER BY 
	ul.time_applied DESC;

/*Show all failed upadates*/
SELECT 
    ul.update_id,
    m.machine_id,
    m.machine_status,
    p.version_number,
    p.package_signature,
    ul.update_status,
    ul.time_applied
FROM 
    Update_Logs ul
JOIN 
    Machines m ON ul.machine_id = m.machine_id
JOIN 
    Packages p ON ul.package_id = p.package_id
WHERE 
    ul.update_status = 'Failed';

/*Show all currently pending updates*/
SELECT 
    ul.update_id,
    ul.machine_id,
    m.machine_status,
    ul.package_id,
    p.version_number,
    p.package_signature,
    ul.update_status,
    ul.time_applied
FROM 
    Update_Logs ul
JOIN 
    Machines m ON ul.machine_id = m.machine_id
JOIN 
    Packages p ON ul.package_id = p.package_id
WHERE 
    ul.update_status = 'Pending'
    AND NOT EXISTS (
        SELECT 1 
        FROM Update_Logs ul2
        WHERE ul2.machine_id = ul.machine_id
        AND ul2.package_id = ul.package_id
        AND ul2.time_applied > ul.time_applied
        AND ul2.update_status IN ('Completed', 'Failed')
    );

/*Show the number of updates per machine*/
SELECT 
    m.machine_id, COUNT(ul.update_id) AS update_count
FROM 
    Machines m
LEFT JOIN 
    Update_Logs ul ON m.machine_id = ul.machine_id
GROUP BY 
    m.machine_id;

/*Show all updates for a specific machine*/
SELECT 
    ul.update_id, p.version_number, ul.update_status, ul.time_applied
FROM 
    Update_Logs ul
JOIN 
    Packages p ON ul.package_id = p.package_id
WHERE 
    ul.machine_id = 1;  

/*Licence Management Queries*/

/*Show all valid licences*/
SELECT 
    l.license_key, m.machine_id, l.validity_status, l.expiration_date
FROM 
    Licences l
JOIN 
    Machines m ON l.machine_id = m.machine_id
WHERE 
    l.validity_status = 'Valid'
    AND l.expiration_date > CURRENT_TIMESTAMP;

/*Show all expired licences*/
SELECT 
    l.license_key, m.machine_id, l.expiration_date
FROM 
    Licences l
JOIN 
    Machines m ON l.machine_id = m.machine_id
WHERE 
    l.expiration_date <= CURRENT_TIMESTAMP;

/*User and Acces Management Queries*/

/*Show all users with admin access to a machine*/
SELECT 
    u.user_id, u.name, c.access_level, c.access_granted_at
FROM 
    Credentials c
JOIN 
    Users u ON c.user_id = u.user_id
WHERE 
    c.access_level = 'Admin'
    AND c.machine_id = 1;  -- Replace with the specific machine ID

/*Show all machines a specific user has access to*/
SELECT 
    m.machine_id, c.access_level, c.access_granted_at
FROM 
    Machines m
JOIN 
    Credentials c ON m.machine_id = c.machine_id
WHERE 
    c.user_id = 2; 

/*Show access logs for a specific user*/
SELECT 
    al.log_id, al.machine_id, al.action, al.time_performed
FROM 
    Action_Logs al
WHERE 
    al.user_id = 3;  -- Replace with the user ID

/*Machine Monitoring and Status Queries*/

/*Show all alive machines*/
SELECT * FROM Machines WHERE machine_status = 'Alive';

/*Show all machines with high memory usage*/
SELECT * FROM Machines WHERE memory_usage > 80.0;

/*Show all idle machines*/
SELECT * FROM Machines WHERE activity_status = 'Idle';

/*Show all offline machines*/
SELECT * FROM Machines WHERE machine_status = 'Shutdown' OR activity_status = 'Offline';


/*Package Version queries*/

/*Show all versions running a specific package version*/
SELECT 
    m.machine_id, p.version_number
FROM 
    Machines m
JOIN 
    Update_Logs ul ON m.machine_id = ul.machine_id
JOIN 
    Packages p ON ul.package_id = p.package_id
WHERE 
    p.version_number = 1.2;
	
/*Action Log and Auditing Queries*/

/*Show all actions performed on a specific machine*/
SELECT 
    al.log_id, u.name, al.action, al.time_performed
FROM 
    Action_Logs al
JOIN 
    Users u ON al.user_id = u.user_id
WHERE 
    al.machine_id = 1;  -- Replace with the machine ID

/*Show the most recent actions performed by each user*/
SELECT 
    al.user_id, u.name, al.action, MAX(al.time_performed) AS last_action_time
FROM 
    Action_Logs al
JOIN 
    Users u ON al.user_id = u.user_id
GROUP BY 
    al.user_id, u.name;

/*Show all actions related to update rollbacks*/
SELECT 
    al.log_id, al.machine_id, u.name, al.time_performed
FROM 
    Action_Logs al
JOIN 
    Users u ON al.user_id = u.user_id
WHERE 
    al.action = 'rollback_initiated';

/*Role-Based Access Control*/

-- Create Roles
CREATE ROLE 'Admin';
CREATE ROLE 'Engineer';

-- Admin Role: Full Permissions on All Tables
GRANT ALL PRIVILEGES ON upflux.* TO 'Admin';

-- Engineer Role: Specific Permissions
GRANT SELECT, INSERT, UPDATE ON upflux.Machines TO 'Engineer';
GRANT SELECT, INSERT, UPDATE ON upflux.Update_Logs TO 'Engineer';
GRANT SELECT, INSERT, UPDATE ON upflux.Packages TO 'Engineer';

-- Create Roles
CREATE ROLE 'Admin', 'Engineer';

-- Admin Role: Full Permissions on All Tables
GRANT ALL PRIVILEGES ON upflux_db.* TO 'Admin';

-- Engineer Role: Specific Permissions
GRANT SELECT ON upflux_db.Machines TO 'Engineer';
GRANT SELECT ON upflux_db.Update_Logs TO 'Engineer';
GRANT SELECT, INSERT, UPDATE ON upflux_db.Packages TO 'Engineer';

DELIMITER //
CREATE PROCEDURE CreateUser(
    IN p_name VARCHAR(255), 
    IN p_email VARCHAR(255), 
    IN p_role ENUM('Admin', 'Engineer'),
    IN p_password VARCHAR(255)
)
BEGIN
    DECLARE v_user_id VARCHAR(50);
    DECLARE v_username VARCHAR(255);
    
    -- Generate a username based on the email (before '@')
    SET v_username = SUBSTRING_INDEX(p_email, '@', 1);

    -- Insert into Users table
    INSERT INTO Users (name, email, role) 
    VALUES (p_name, p_email, p_role);

    -- Get the last inserted user_id
    SET v_user_id = LAST_INSERT_ID();

    -- For Admin, insert into Admin_Details with password hash
    IF p_role = 'Admin' THEN
        INSERT INTO Admin_Details (user_id, password_hash) 
        VALUES (v_user_id, SHA2(p_password, 256));
    END IF;

    -- Create a MySQL user for the newly added user
    SET @create_user_query = CONCAT(
        'CREATE USER \'', v_username, '\'@\'%\' IDENTIFIED BY \'', p_password, '\';'
    );
    PREPARE stmt FROM @create_user_query;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;

    -- Grant the appropriate role based on the user's role
    IF p_role = 'Admin' THEN
        SET @grant_role_query = CONCAT(
            'GRANT \'Admin\' TO \'', v_username, '\'@\'%\';'
        );
    ELSEIF p_role = 'Engineer' THEN
        SET @grant_role_query = CONCAT(
            'GRANT \'Engineer\' TO \'', v_username, '\'@\'%\';'
        );
    END IF;

    PREPARE stmt FROM @grant_role_query;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;

    -- Activate roles for the new user as they will not take effect unless activated
    SET @activate_role_query = CONCAT(
        'SET DEFAULT ROLE ALL TO \'', v_username, '\'@\'%\';'
    );
    PREPARE stmt FROM @activate_role_query;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;
END //
DELIMITER ;

/* Attribute-Based Access Control */
DELIMITER //

CREATE PROCEDURE CheckMachineAccess(
    IN p_user_id VARCHAR(50),
    IN p_machine_id VARCHAR(255),
    OUT p_access_granted BOOLEAN
)
BEGIN
    DECLARE v_user_role ENUM('Admin', 'Engineer');
    DECLARE v_credential_valid BOOLEAN;
    DECLARE v_within_working_hours BOOLEAN;

    -- Get user's role (RBAC check)
    SELECT role INTO v_user_role FROM Users WHERE user_id = p_user_id;

    -- Check if credential is valid (ABAC: not expired AND no newer revocation)
    SELECT EXISTS (
        SELECT 1 
        FROM Credentials c
        WHERE 
            c.user_id = p_user_id 
            AND c.machine_id = p_machine_id 
            AND c.expires_at > NOW() -- Credential not expired
            -- Ensure no revocation after this credential was granted
            AND NOT EXISTS (
                SELECT 1 
                FROM Revoked_Tokens rt
                WHERE 
                    rt.user_id = p_user_id 
                    AND rt.revoked_at > c.access_granted_at
            )
    ) INTO v_credential_valid;

    -- Check time constraint (ABAC: 8 AM to 6 PM)
    SET v_within_working_hours = (CURTIME() BETWEEN '08:00:00' AND '18:00:00');

    -- Apply ABAC policy based on role
    CASE 
        WHEN v_user_role = 'Admin' THEN
            -- Admins need valid credentials but no time restriction
            SET p_access_granted = v_credential_valid;
        WHEN v_user_role = 'Engineer' THEN
            -- Engineers need valid credentials AND working hours
            SET p_access_granted = v_credential_valid AND v_within_working_hours;
        ELSE
            SET p_access_granted = FALSE;
    END CASE;
END //

DELIMITER ;

-- Testing Stored Procedure

-- Example Admin User
CALL CreateUser('John Doe', 'john@example.com', 'Admin', 'SecurePassword123455');

-- Admin_Details should be populated with john's info
Select * from Admin_Details;

-- Example Engineer User
CALL CreateUser('Mark Doe', 'mark123@example.com', 'Engineer', '');

-- Testing RBAC

-- Checking that correct permissions are assigned for each role
SHOW GRANTS FOR 'Admin';
SHOW GRANTS FOR 'Engineer';

-- Should show that john has the ADMIN role
SHOW GRANTS FOR 'john'@'%';

-- Should show that mark has the Engineer role
SHOW GRANTS FOR 'mark123'@'%';

-- Stored Procedure for handling user login
DELIMITER //

CREATE PROCEDURE UserLogin(
    IN p_user_id VARCHAR(50) 
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        -- Handle errors
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error in UserLogin procedure';
    END;

    -- Clear any existing session data for this connection
    DELETE FROM User_Context;

    -- Insert the current user's ID into the temporary table
    INSERT INTO User_Context (user_id) VALUES (p_user_id);
END//

DELIMITER ;

-- Stored Procedures for logging actions performed by users
DELIMITER //

CREATE PROCEDURE LogAction(
    IN p_action_type VARCHAR(10),
    IN p_entity_name VARCHAR(255))
BEGIN
    DECLARE v_user_id VARCHAR(50);
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
    BEGIN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error in LogAction procedure';
    END;
    
    -- Get the current user's ID from the temporary table
    SELECT user_id INTO v_user_id
    FROM User_Context
    LIMIT 1;
    
    -- Log the action
    INSERT INTO Action_Logs (user_id, action_type, entity_name, time_performed)
    VALUES (v_user_id, p_action_type, p_entity_name, NOW());
END//

DELIMITER ;

-- Stored procedure for clearing session data once the user logs out
DELIMITER //

CREATE PROCEDURE UserLogout()
BEGIN
    -- Clear the current user's session
    DELETE FROM User_Context;
END//

DELIMITER ;

-- Triggers with SIGNAL Error Handling
DELIMITER //
CREATE TRIGGER DeleteAdminDetails
AFTER DELETE ON Users
FOR EACH ROW
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
    BEGIN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error in DeleteAdminDetails trigger';
    END;
    
    IF OLD.role = 'Admin' THEN
        DELETE FROM Admin_Details WHERE user_id = OLD.user_id;
    END IF;
END //
DELIMITER ;

-- Generalized Logging Triggers with Error Handling
DELIMITER //
CREATE TRIGGER LogUserInsert AFTER INSERT ON Users FOR EACH ROW
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
    BEGIN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error in LogUserInsert trigger';
    END;
    CALL LogAction('CREATE', 'Users');
END //

CREATE TRIGGER LogUserUpdate AFTER UPDATE ON Users FOR EACH ROW
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
    BEGIN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error in LogUserUpdate trigger';
    END;
    CALL LogAction('UPDATE', 'Users');
END //

CREATE TRIGGER LogUserDelete AFTER DELETE ON Users FOR EACH ROW
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
    BEGIN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error in LogUserDelete trigger';
    END;
    CALL LogAction('DELETE', 'Users');
END //

CREATE TRIGGER LogMachineInsert AFTER INSERT ON Machines FOR EACH ROW
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
    BEGIN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error in LogMachineInsert trigger';
    END;
    CALL LogAction('CREATE', 'Machines');
END //

CREATE TRIGGER LogMachineUpdate AFTER UPDATE ON Machines FOR EACH ROW
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
    BEGIN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error in LogMachineUpdate trigger';
    END;
    CALL LogAction('UPDATE', 'Machines');
END //

CREATE TRIGGER LogMachineDelete AFTER DELETE ON Machines FOR EACH ROW
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
    BEGIN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error in LogMachineDelete trigger';
    END;
    CALL LogAction('DELETE', 'Machines');
END //

CREATE TRIGGER LogLicenceInsert AFTER INSERT ON Licences FOR EACH ROW
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
    BEGIN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error in LogLicenceInsert trigger';
    END;
    CALL LogAction('CREATE', 'Licences');
END //

CREATE TRIGGER LogLicenceUpdate AFTER UPDATE ON Licences FOR EACH ROW
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
    BEGIN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error in LogLicenceUpdate trigger';
    END;
    CALL LogAction('UPDATE', 'Licences');
END //

CREATE TRIGGER LogLicenceDelete AFTER DELETE ON Licences FOR EACH ROW
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
    BEGIN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error in LogLicenceDelete trigger';
    END;
    CALL LogAction('DELETE', 'Licences');
END //

CREATE TRIGGER LogCredentialInsert AFTER INSERT ON Credentials FOR EACH ROW
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
    BEGIN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error in LogCredentialInsert trigger';
    END;
    CALL LogAction('CREATE', 'Credentials');
END //

CREATE TRIGGER LogCredentialUpdate AFTER UPDATE ON Credentials FOR EACH ROW
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
    BEGIN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error in LogCredentialUpdate trigger';
    END;
    CALL LogAction('UPDATE', 'Credentials');
END //

CREATE TRIGGER LogCredentialDelete AFTER DELETE ON Credentials FOR EACH ROW
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
    BEGIN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error in LogCredentialDelete trigger';
    END;
    CALL LogAction('DELETE', 'Credentials');
END //

CREATE TRIGGER LogPackageInsert AFTER INSERT ON Packages FOR EACH ROW
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
    BEGIN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error in LogPackageInsert trigger';
    END;
    CALL LogAction('CREATE', 'Packages');
END //

CREATE TRIGGER LogPackageUpdate AFTER UPDATE ON Packages FOR EACH ROW
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
    BEGIN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error in LogPackageUpdate trigger';
    END;
    CALL LogAction('UPDATE', 'Packages');
END //

CREATE TRIGGER LogPackageDelete AFTER DELETE ON Packages FOR EACH ROW
BEGIN
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION
    BEGIN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error in LogPackageDelete trigger';
    END;
    CALL LogAction('DELETE', 'Packages');
END //

DELIMITER ;

-- Check Existing Triggers
SELECT 
    TRIGGER_NAME,
    EVENT_MANIPULATION AS EVENT,
    EVENT_OBJECT_TABLE AS TABLE_NAME,
    ACTION_STATEMENT AS ACTION
FROM 
    information_schema.TRIGGERS
WHERE 
    TRIGGER_SCHEMA = 'upflux'; 
	
-- Views

-- View details about engineer users
CREATE VIEW Engineer_Users AS
SELECT 
    user_id,
    name,
    email,
    last_login
FROM Users
WHERE role = 'Engineer';

-- View details about admin users
CREATE VIEW Admin_Users AS
SELECT 
    U.user_id,
    U.name,
    U.email,
    U.last_login
FROM Users U
JOIN Admin_Details A ON U.user_id = A.user_id
WHERE U.role = 'Admin';

-- View the list of machines which users have access to
CREATE VIEW User_Access AS
SELECT 
    c.user_id, 
    u.name AS user_name, 
    c.machine_id, 
    c.access_granted_at, 
    c.expires_at, 
    c.access_granted_by
FROM Credentials c
JOIN Users u ON c.user_id = u.user_id;

-- View user activity logs
CREATE VIEW User_Action_Logs AS
SELECT 
    al.log_id, 
    u.name AS user_name, 
    al.action_type, 
    al.entity_name, 
    al.time_performed
FROM Action_Logs al
JOIN Users u ON al.user_id = u.user_id;

-- View the latest package installed on each machine
CREATE VIEW Latest_Package_Versions AS
SELECT 
    ul.machine_id, 
    p.version_number, 
    MAX(ul.time_applied) AS latest_update_time
FROM Update_Logs ul
JOIN Packages p ON ul.package_id = p.package_id
WHERE ul.update_status = 'Completed'
GROUP BY ul.machine_id, p.version_number;

-- View the list of apps and versions on each machine
CREATE VIEW Application_Status AS
SELECT 
    a.machine_id, 
    a.app_name, 
    a.current_version
FROM Applications a;

-- View all machines with currently expired licences
CREATE VIEW Expired_Licences AS
SELECT 
    m.machine_id,
    m.machine_name,
    m.ip_address,
    l.licence_key,
    l.expiration_date
FROM 
    Machines m
JOIN 
    Licences l ON m.machine_id = l.machine_id
WHERE 
    l.expiration_date <= CURRENT_TIMESTAMP;

-- View all machines with licences which are expiring soon
CREATE VIEW Licences_Expiring_Soon AS
SELECT 
    m.machine_id,
    m.machine_name,
    m.ip_address,
    l.licence_key,
    l.expiration_date
FROM 
    Machines m
JOIN 
    Licences l ON m.machine_id = l.machine_id
WHERE 
    l.expiration_date BETWEEN CURRENT_TIMESTAMP AND CURRENT_TIMESTAMP + INTERVAL '30 days';

-- View relevant application data for an engineer
CREATE VIEW Application_Details AS
SELECT 
    a.app_name,
    a.current_version,
    u1.name AS added_by,
    av.date AS last_updated,
    u2.name AS updated_by
FROM Applications a
LEFT JOIN (
    SELECT av1.app_id, av1.updated_by, av1.date
    FROM Application_Versions av1
    WHERE av1.date = (SELECT MAX(av2.date) FROM Application_Versions av2 WHERE av1.app_id = av2.app_id)
) av ON a.app_id = av.app_id
LEFT JOIN Users u1 ON a.added_by = u1.user_id
LEFT JOIN Users u2 ON av.updated_by = u2.user_id;
