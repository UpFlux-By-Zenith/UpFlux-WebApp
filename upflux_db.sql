/*CREATE DATABASE upflux*/

USE upflux;

/*Create Table Statements*/
CREATE TABLE Users (
    user_id INT NOT NULL AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    PRIMARY KEY (user_id)
);

CREATE TABLE Machines (
    machine_id INT NOT NULL AUTO_INCREMENT,
    machine_status ENUM('Alive', 'Shutdown', 'Unknown') NOT NULL,
    memory_usage FLOAT,
    activity_status ENUM('Busy', 'Idle', 'Offline'),
    PRIMARY KEY (machine_id)
);

CREATE TABLE Packages (
    package_id INT NOT NULL AUTO_INCREMENT,
    version_number FLOAT NOT NULL,
    package_size FLOAT NOT NULL,
    package_signature VARCHAR(255),
    release_date TIMESTAMP NOT NULL,
    PRIMARY KEY (package_id)
);

CREATE TABLE Licences (
    licence_key VARCHAR(255) NOT NULL,
    machine_id INT NOT NULL,
    validity_status VARCHAR(50) NOT NULL,
    expiration_date TIMESTAMP NOT NULL,
    PRIMARY KEY (licence_key),
    FOREIGN KEY (machine_id) REFERENCES Machines(machine_id)
);

CREATE TABLE Credentials (
    machine_id INT NOT NULL,
    user_id INT NOT NULL,
    access_level ENUM('Read', 'Write', 'Admin') NOT NULL,
    access_granted_at TIMESTAMP,
    PRIMARY KEY (machine_id, user_id),
    FOREIGN KEY (machine_id) REFERENCES Machines(machine_id),
    FOREIGN KEY (user_id) REFERENCES Users(user_id)
);

CREATE TABLE Update_Logs (
    update_id INT NOT NULL AUTO_INCREMENT,
    machine_id INT NOT NULL,
    package_id INT NOT NULL,
    update_status ENUM('Pending', 'Completed', 'Failed') NOT NULL,
    time_applied TIMESTAMP,
    PRIMARY KEY (update_id),
    FOREIGN KEY (machine_id) REFERENCES Machines(machine_id),
    FOREIGN KEY (package_id) REFERENCES Packages(package_id)
);

CREATE TABLE Action_Logs (
    log_id INT NOT NULL AUTO_INCREMENT,
    machine_id INT NOT NULL,
    user_id INT NOT NULL,
    action ENUM('update_initiated', 'update_scheduled', 'rollback_initiated') NOT NULL,
    time_performed TIMESTAMP,
    PRIMARY KEY (log_id),
    FOREIGN KEY (machine_id) REFERENCES Machines(machine_id),
    FOREIGN KEY (user_id) REFERENCES Users(user_id)
);

/*Show all tables present in the database*/
SHOW TABLES;

DESCRIBE Machines;

/*Insert Sample data for each table*/
INSERT INTO Machines (machine_status, memory_usage, activity_status) VALUES
('Alive', 50.3, 'Busy'),
('Shutdown', 0, 'Offline'),
('Alive', 70.1, 'Idle'),
('Unknown', NULL, 'Offline');

INSERT INTO Users (user_id, name, email, password_hash) VALUES
(1, 'Alice', 'alice@example.com', '5f4dcc3b5aa765d61d8327deb882cf99'),
(2, 'Bob', 'bob@example.com', '5f4dcc3b5aa765d61d8327deb882cf99'),
(3, 'Charlie', 'charlie@example.com', '5f4dcc3b5aa765d61d8327deb882cf99');

Select * from Users;

INSERT INTO Licences (licence_key, machine_id, validity_status, expiration_date) VALUES
('ABC123', 1, 'Valid', '2025-12-31 23:59:59'),
('DEF456', 2, 'Expired', '2023-08-15 12:00:00'),
('GHI789', 3, 'Valid', '2026-07-10 10:00:00');

Select * from Licences;

INSERT INTO Credentials (machine_id, user_id, access_level, access_granted_at) VALUES
(1, 1, 'Admin', '2023-01-01 12:00:00'),
(2, 2, 'Write', '2023-03-15 09:30:00'),
(3, 3, 'Read', '2023-05-20 11:45:00');

Select * from Credentials;

INSERT INTO Packages (version_number, package_size, package_signature, release_date) VALUES
(1.0, 15.5, 'abcde12345', '2023-07-01 14:30:00'),
(1.1, 20.2, 'fghij67890', '2023-08-15 16:00:00'),
(1.2, 25.3, 'klmno54321', '2023-09-30 12:15:00');

Select * from Packages;

INSERT INTO Update_Logs (machine_id, package_id, update_status, time_applied) VALUES
(1, 1, 'Completed', '2023-07-02 15:00:00'),
(2, 2, 'Pending', '2023-08-16 10:00:00'),
(3, 3, 'Failed', '2023-10-01 13:45:00'),
(2, 3, 'Completed', '2023-08-16 10:00:00'),
(3, 2, 'Completed', '2023-10-01 13:45:00');

Select * from Update_Logs;

INSERT INTO Action_Logs (machine_id, user_id, action, time_performed) VALUES
(1, 1, 'update_initiated', '2023-07-01 14:45:00'),
(2, 2, 'update_scheduled', '2023-08-14 15:00:00'),
(3, 3, 'rollback_initiated', '2023-10-02 10:30:00');

/*Basic Queries*/

/*Show all users*/
SELECT * FROM Machines;

/*Show all users*/
SELECT * FROM Users;

/*Show all packages*/
SELECT * FROM Packages;

/*Show all licences*/

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



