/*CREATE DATABASE upflux*/

USE upflux;

/*Create Table Statements*/

-- Create Machines Table
CREATE TABLE Machines (
    machine_id VARCHAR(255) PRIMARY KEY,
    date_added TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create Users Table
CREATE TABLE Users (
    user_id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    role ENUM('Admin', 'Engineer') NOT NULL
);

-- Create Admin_Details Table 
CREATE TABLE Admin_Details (
    admin_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
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
    user_id INT NOT NULL,                  
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
    user_id INT NOT NULL,
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
    user_id INT NOT NULL,
    action_type ENUM('CREATE', 'UPDATE', 'DELETE') NOT NULL,
	entity_name VARCHAR(255) NOT NULL,
    time_performed TIMESTAMP NOT NULL,
    FOREIGN KEY (user_id) REFERENCES Users(user_id)
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
INSERT INTO Credentials (user_id, machine_id, access_granted_at, expires_at) VALUES
(1, '1', '2023-01-01 12:00:00', '2023-01-31 12:00:00'),
(2, '2', '2023-03-15 09:30:00', '2023-04-14 09:30:00'),
(3, '3', '2023-05-20 11:45:00', '2023-06-19 11:45:00');


-- View Credentials table data
SELECT * FROM Credentials;

/* Inserting data into Packages */
INSERT INTO Packages (version_number, package_size, package_signature, release_date) VALUES
(1.0, 15.5, 'abcde12345', '2023-07-01 14:30:00'),
(1.1, 20.2, 'fghij67890', '2023-08-15 16:00:00'),
(1.2, 25.3, 'klmno54321', '2023-09-30 12:15:00');

-- View Packages table data
SELECT * FROM Packages;

/* Inserting data into Update Logs */
INSERT INTO Update_Logs (package_id, user_id, machine_id, update_status, time_applied) VALUES
(1, 1, '1', 'Completed', '2023-07-02 15:00:00'),
(2, 2, '2', 'Pending', '2023-08-16 10:00:00'),
(3, 3, '3', 'Failed', '2023-10-01 13:45:00'),
(3, 2, '2', 'Completed', '2023-08-16 10:00:00'),
(2, 3, '3', 'Completed', '2023-10-01 13:45:00');

-- View Update Logs table data
SELECT * FROM Update_Logs;

/* Inserting data into Action Logs */
INSERT INTO Action_Logs (user_id, action_type, entity_name, time_performed) VALUES
(1, 'CREATE', 'Users', '2023-07-01 14:45:00'),
(2, 'UPDATE', 'Credentials', '2023-08-14 15:00:00'),
(3, 'DELETE', 'Machines', '2023-10-02 10:30:00');

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
    DECLARE v_user_id INT;
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
    IN p_user_id INT,
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
SET @current_user_id = 1;

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

CREATE TRIGGER LogLicenceInsert
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
    CALL LogAction(@current_user_id, 'DELETE', 'Licenes');
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
	
-- Indexes 

-- Index to speed up queries filtering or joining on user_id in the Credentials table
CREATE INDEX idx_credentials_user_id ON Credentials(user_id);

-- Index to speed up queries filtering or joining on machine_id in the Credentials table
CREATE INDEX idx_credentials_machine_id ON Credentials(machine_id);

-- Index to speed up queries filtering or joining on machine_id in the Licences table
CREATE INDEX idx_licences_machine_id ON Licences(machine_id);

-- Index to speed up queries filtering or joining on machine_id in the Update_Logs table
CREATE INDEX idx_update_logs_machine_id ON Update_Logs(machine_id);

-- Index to speed up queries filtering or joining on package_id in the Update_Logs table
CREATE INDEX idx_update_logs_package_id ON Update_Logs(package_id);

-- Composite index to optimize queries involving both machine_id and package_id in Update_Logs
CREATE INDEX idx_update_logs_machine_package ON Update_Logs(machine_id, package_id);

-- Composite index to optimize queries involving both user_id and machine_id in Credentials
CREATE INDEX idx_credentials_user_machine ON Credentials(user_id, machine_id);