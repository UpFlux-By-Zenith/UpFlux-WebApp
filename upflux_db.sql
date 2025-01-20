/*CREATE DATABASE upflux*/

USE upflux;

/*Create Table Statements*/

-- Create Machines Table
CREATE TABLE Machines (
    machine_id VARCHAR(255) PRIMARY KEY,
    date_added TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ip_address VARCHAR(15) NOT NULL 
);

-- Create Users Table
CREATE TABLE Users (
    user_id VARCHAR(50) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    role ENUM('Admin', 'Engineer') NOT NULL,
	last_login TIMESTAMP
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
    validity_status VARCHAR(50) NOT NULL,
    expiration_date TIMESTAMP NOT NULL,
    FOREIGN KEY (machine_id) REFERENCES Machines(machine_id)
);

-- Create Credentials Table
CREATE TABLE Credentials (
    credential_id INT AUTO_INCREMENT PRIMARY KEY, 
    user_id VARCHAR(50) NOT NULL,                  
    machine_id VARCHAR(255) NOT NULL,                 
    access_granted_at TIMESTAMP NOT NULL,   
    expires_at TIMESTAMP NOT NULL,           
    FOREIGN KEY (user_id) REFERENCES Users(user_id),
    FOREIGN KEY (machine_id) REFERENCES Machines(machine_id),
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
    FOREIGN KEY (added_by) REFERENCES Users(user_id),
    FOREIGN KEY (machine_id) REFERENCES Machines(machine_id) 
);

-- Create Application_Versions Table
CREATE TABLE Application_Versions (
    version_id INT AUTO_INCREMENT PRIMARY KEY,  
    app_id INT NOT NULL,
    version_name VARCHAR(50) NOT NULL, 
    updated_by VARCHAR(50) NOT NULL,
    date TIMESTAMP NOT NULL,
    FOREIGN KEY (app_id) REFERENCES Applications(app_id),
    FOREIGN KEY (updated_by) REFERENCES Users(user_id)
);

/*Show all tables present in the database*/
SHOW TABLES;

DESCRIBE Machines;

-- Inserting data into machine table
INSERT INTO Machines (machine_id) VALUES
('1'),
('2'),
('3');


-- View Machines table data
SELECT * FROM Machines;

/* Inserting data into Users */
INSERT INTO Users (name, email, role) VALUES
('Alice', 'alice@example.com', 'Admin'),
('Bob', 'bob@example.com', 'Engineer'),
('Charlie', 'charlie@example.com', 'Engineer');

-- View Users table data
SELECT * FROM Users;

/* Inserting data into Licences */
INSERT INTO Licences (licence_key, machine_id, validity_status, expiration_date) VALUES
('ABC123', '1', 'Valid', '2025-12-31 23:59:59'),
('DEF456', '2', 'Expired', '2023-08-15 12:00:00'),
('GHI789', '3', 'Valid', '2026-07-10 10:00:00');

-- View Licences table data
SELECT * FROM Licences;

/* Inserting data into Credentials */
-- Insert into Machines
INSERT INTO Machines (machine_id, date_added) 
VALUES ('1', '2025-01-01 12:00:00'),
       ('2', '2025-01-02 14:30:00'),
       ('3', '2025-01-03 09:45:00');

-- Insert into Users
INSERT INTO Users (user_id, name, email, role, last_login) 
VALUES ('a1', 'Alice', 'alice@upflux.com', 'Admin', '2025-01-07 10:00:00'),
       ('e1', 'Eve', 'eve@upflux.com', 'Engineer', '2025-01-07 11:00:00'),
       ('e2', 'Eli', 'eli@upflux.com', 'Engineer', '2025-01-06 09:30:00');

-- Insert into Admin_Details
INSERT INTO Admin_Details (admin_id, user_id, password_hash)
VALUES ('a1', 'a1', 'hash_for_alice');

-- Insert into Licences
INSERT INTO Licences (licence_key, machine_id, validity_status, expiration_date)
VALUES ('LIC-12345', '1', 'Valid', '2026-01-01 12:00:00'),
       ('LIC-23456', '2', 'Expired', '2024-12-31 23:59:59'),
       ('LIC-34567', '3', 'Revoked', '2025-06-01 00:00:00');

-- Insert into Credentials
INSERT INTO Credentials (user_id, machine_id, access_granted_at, expires_at)
VALUES ('e1', '1', '2025-01-01 12:30:00', '2025-01-10 12:30:00'),
       ('e2', '2', '2025-01-02 15:00:00', '2025-01-12 15:00:00'),
       ('e1', '3', '2025-01-03 10:00:00', '2025-01-13 10:00:00');

-- Insert into Packages
INSERT INTO Packages (version_number, package_size, package_signature, release_date)
VALUES (1.0, 25.5, 'sig123', '2024-12-01 12:00:00'),
       (1.1, 30.0, 'sig124', '2025-01-01 12:00:00'),
       (1.2, 35.2, 'sig125', '2025-01-07 12:00:00');

-- Insert into Update_Logs
INSERT INTO Update_Logs (package_id, user_id, machine_id, update_status, time_applied)
VALUES (1, 'e1', '1', 'Completed', '2025-01-01 13:00:00'),
       (2, 'e2', '2', 'Failed', '2025-01-02 16:00:00'),
       (3, 'e1', '3', 'Pending', '2025-01-07 14:00:00');

-- Insert into Action_Logs
INSERT INTO Action_Logs (user_id, action_type, entity_name, time_performed)
VALUES ('a1', 'CREATE', 'Machine', '2025-01-01 12:10:00'),
       ('e1', 'UPDATE', 'Licence', '2025-01-02 16:30:00'),
       ('e2', 'DELETE', 'Credential', '2025-01-03 11:00:00');

-- Insert into Applications
INSERT INTO Applications (machine_id, app_name, added_by, current_version)
VALUES ('1', 'App One', 'a1', '1.0.0'),
       ('2', 'App Two', 'e1', '2.1.0'),
       ('3', 'App Three', 'e2', '1.3.5');

-- Insert into Application_Versions
INSERT INTO Application_Versions (app_id, version_name, updated_by, date)
VALUES ('1', '1.1.0', 'e1', '2025-01-05 14:00:00'),
       ('2', '2.2.0', 'a1', '2025-01-06 16:30:00'),
       ('3', '1.4.0', 'e2', '2025-01-07 11:00:00');

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

--Stored Procedures

DELIMITER //

CREATE PROCEDURE LogAction(
    IN p_user_id VARCHAR(50),
    IN p_action_type VARCHAR(10),
    IN p_entity_name VARCHAR(255)
)
BEGIN
    INSERT INTO Action_Logs (user_id, action_type, entity_name, time_performed)
    VALUES (p_user_id, p_action_type, p_entity_name, NOW());
END //

DELIMITER ;

-- Triggers

-- For testing triggers as a specific user:
SET @current_user_id = a1;

-- Trigger for deleting a users associated admin details whenever they are removed from the users table
DELIMITER //
CREATE TRIGGER DeleteAdminDetails
AFTER DELETE ON Users
FOR EACH ROW
BEGIN
    -- Check if the deleted user was an Admin
    IF OLD.role = 'Admin' THEN
        -- Delete corresponding entry from Admin_Details
        DELETE FROM Admin_Details
        WHERE user_id = OLD.user_id;
    END IF;
END //
DELIMITER ;

-- Trigger for adding entry to action_logs when a user performs an Insert
DELIMITER //

CREATE TRIGGER LogUserInsert
AFTER INSERT ON Users
FOR EACH ROW
BEGIN
    CALL LogAction(@current_user_id, 'CREATE', 'Users');
END //

DELIMITER ;

-- Trigger for adding entry to action_logs when a user performs an Update
DELIMITER //

CREATE TRIGGER LogUserUpdate
AFTER UPDATE ON Users
FOR EACH ROW
BEGIN
    CALL LogAction(@current_user_id, 'UPDATE', 'Users');
END //

DELIMITER ;


-- Trigger for adding entry to action_logs when a user performs a Delete
DELIMITER //

CREATE TRIGGER LogUserDelete
AFTER DELETE ON Users
FOR EACH ROW
BEGIN
    CALL LogAction(@current_user_id, 'DELETE', 'Users');
END //

DELIMITER ;

-- Machines Triggers

-- Trigger for adding entry to action_logs when a user performs an Insert on Machines
DELIMITER //

CREATE TRIGGER LogMachineInsert
AFTER INSERT ON Machines
FOR EACH ROW
BEGIN
    CALL LogAction(@current_user_id, 'CREATE', 'Machines');
END //

DELIMITER ;

-- Trigger for adding entry to action_logs when a user performs an Update on Machines
DELIMITER //

CREATE TRIGGER LogMachineUpdate
AFTER UPDATE ON Machines
FOR EACH ROW
BEGIN
    CALL LogAction(@current_user_id, 'UPDATE', 'Machines');
END //

DELIMITER ;

-- Trigger for adding entry to action_logs when a user performs a Delete on Machines
DELIMITER //

CREATE TRIGGER LogMachineDelete
AFTER DELETE ON Machines
FOR EACH ROW
BEGIN
    CALL LogAction(@current_user_id, 'DELETE', 'Machines');
END //

DELIMITER ;

-- Licences

-- Trigger for adding entry to action_logs when a user performs an Insert on Licences
DELIMITER //

CREATE TRIGGER LogLicenseInsert
AFTER INSERT ON Licences
FOR EACH ROW
BEGIN
    CALL LogAction(@current_user_id, 'CREATE', 'Licences');
END //

DELIMITER ;

-- Trigger for adding entry to action_logs when a user performs an Update on Licences
DELIMITER //

CREATE TRIGGER LogLicenceUpdate
AFTER UPDATE ON Licences
FOR EACH ROW
BEGIN
    CALL LogAction(@current_user_id, 'UPDATE', 'Licences');
END //

DELIMITER ;

-- Trigger for adding entry to action_logs when a user performs a Delete on Licences
DELIMITER //

CREATE TRIGGER LogLicenceDelete
AFTER DELETE ON Licences
FOR EACH ROW
BEGIN
    CALL LogAction(@current_user_id, 'DELETE', 'Licences');
END //

DELIMITER ;

-- Credentials

-- Trigger for adding entry to action_logs when a user performs an Insert on Credentials
DELIMITER //

CREATE TRIGGER LogCredentialInsert
AFTER INSERT ON Credentials
FOR EACH ROW
BEGIN
    CALL LogAction(@current_user_id, 'CREATE', 'Credentials');
END //

DELIMITER ;

-- Trigger for adding entry to action_logs when a user performs an Update on Credentials
DELIMITER //

CREATE TRIGGER LogCredentialUpdate
AFTER UPDATE ON Credentials
FOR EACH ROW
BEGIN
    CALL LogAction(@current_user_id, 'UPDATE', 'Credentials');
END //

DELIMITER ;

-- Trigger for adding entry to action_logs when a user performs a Delete on Credentials
DELIMITER //

CREATE TRIGGER LogCredentialDelete
AFTER DELETE ON Credentials
FOR EACH ROW
BEGIN
    CALL LogAction(@current_user_id, 'DELETE', 'Credentials');
END //

DELIMITER ;

-- Packages 

-- Trigger for adding entry to action_logs when a user performs an Insert on Packages
DELIMITER //

CREATE TRIGGER LogPackageInsert
AFTER INSERT ON Packages
FOR EACH ROW
BEGIN
    CALL LogAction(@current_user_id, 'CREATE', 'Packages');
END //

DELIMITER ;

-- Trigger for adding entry to action_logs when a user performs an Update on Packages
DELIMITER //

CREATE TRIGGER LogPackageUpdate
AFTER UPDATE ON Packages
FOR EACH ROW
BEGIN
    CALL LogAction(@current_user_id, 'UPDATE', 'Packages');
END //

DELIMITER ;

-- Trigger for adding entry to action_logs when a user performs a Delete on Packages
DELIMITER //

CREATE TRIGGER LogPackageDelete
AFTER DELETE ON Packages
FOR EACH ROW
BEGIN
    CALL LogAction(@current_user_id, 'DELETE', 'Packages');
END //

DELIMITER ;


-- Trigger for updating the status of a licence to 'Expired'
DELIMITER //

CREATE TRIGGER UpdateLicenseValidity
BEFORE UPDATE ON Licences
FOR EACH ROW
BEGIN
    -- Check if the expiration date has passed, and update the validity_status
    IF NEW.expiration_date < NOW() AND OLD.validity_status != 'Expired' THEN
        SET NEW.validity_status = 'Expired';
    END IF;
END //

DELIMITER ;


-- Trigger for inserting new credentials 
DELIMITER //

CREATE TRIGGER LogCredentialsInsert
AFTER INSERT ON Credentials
FOR EACH ROW
BEGIN
    INSERT INTO Action_Logs (user_id, action_type, entity_name, time_performed)
    VALUES (@current_user_id, 'CREATE', 'Credentials', NOW());
END //

DELIMITER ;

-- Trigger for updating credentials
DELIMITER //

CREATE TRIGGER LogCredentialsUpdate
AFTER UPDATE ON Credentials
FOR EACH ROW
BEGIN
    INSERT INTO Action_Logs (user_id, action_type, entity_name, time_performed)
    VALUES (@current_user_id, 'UPDATE', 'Credentials', NOW());
END //

DELIMITER ;

-- Trigger for deleting credentials
DELIMITER //
CREATE TRIGGER LogCredentialsDelete
AFTER DELETE ON Credentials
FOR EACH ROW
BEGIN
    INSERT INTO Action_Logs (user_id, action_type, entity_name, time_performed)
    VALUES (@current_user_id, 'DELETE', 'Credentials', NOW());
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