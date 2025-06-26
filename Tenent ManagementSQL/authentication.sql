--USE TenentManagement;

--(V.0) authentication table creation
--CREATE TABLE TBL_AUTHENTICATION(
--ID INT PRIMARY KEY IDENTITY,
--USERNAME VARCHAR(50),
--PASSWORD NVARCHAR(100),
--EMAIL VARCHAR(50),
--ROLEID INT NOT NULL FOREIGN KEY REFERENCES TBL_ROLES,
--TOKEN NVARCHAR(100),
--EXPIRY DATETIME,
--);

--(V.0.1) added joineddate and passwordchangingdate
--ALTER TABLE TBL_AUTHENTICATION
--ADD JOINEDDATE DATETIME;
--ALTER TABLE TBL_AUTHENTICATION
--ADD PASSWORDCHANGINGDATE DATETIME;


--(V.0.2) renamed RESETTOKEN into TOKEN
--EXEC sp_rename 'TBL_AUTHENTICATION.RESETTOKEN', 'TOKEN', 'COLUMN';


--(V.0.3) added isVerified for emailVerification
-- 0 for unverified user and 1 for verified user
--ALTER TABLE TBL_AUTHENTICATION ADD ISVERIFIED BIT DEFAULT 0;



CREATE PROCEDURE SP_AUTHENTICATION
@FLAG CHAR = NULL,
@ID INT = NULL,
@USERNAME VARCHAR(50) = NULL,
@FIRSTNAME VARCHAR(50) = NULL,
@MIDDLENAME VARCHAR(50) = NULL,
@LASTNAME VARCHAR(50) = NULL,
@PHONENUMBER VARCHAR(20) = NULL,
@PASSWORD NVARCHAR(100) = NULL,
@EMAIL VARCHAR(50) = NULL,
@ROLEID INT = NULL,
@TOKEN NVARCHAR(100) = NULL,
@EXPIRY DATETIME = NULL
AS
BEGIN
	--FOR LOGIN
	IF @FLAG = 'L'
		BEGIN
		    SELECT A.ID, A.USERNAME, A.PASSWORD, A.EMAIL, R.NAME ROLE 
			FROM TBL_AUTHENTICATION A
			INNER JOIN TBL_ROLES R ON A.ROLEID = R.ID
			WHERE
			A.ID = ISNULL(@ID, A.ID)
			  AND A.EMAIL = ISNULL(@EMAIL, A.EMAIL)
			  AND (@ID IS NOT NULL OR @EMAIL IS NOT NULL);

		--Where clause
		--check ID is null or not. if ID is null then Authentication ID is null else Authentication Id = provided ID.
		--and checks email is null or not. If Email is null then Authentication Email is null else Authentication Email = provided Email.
		-- finally the last and makes sure neither is null
		END

	--FOR REGISTER
	IF @FLAG = 'R'
		BEGIN

		-- checking for existing email
			IF EXISTS (SELECT 1 FROM TBL_AUTHENTICATION WHERE EMAIL = @EMAIL)
				BEGIN
					SELECT 'FAIL' STATUS, 'Email is already taken.' MSG;
				END
		--checking for existing username
			IF EXISTS (SELECT 1 FROM TBL_AUTHENTICATION WHERE USERNAME = @USERNAME)
				BEGIN
					SELECT 'FAIL' STATUS, 'Username is already taken.' MSG;
				END
			ELSE
				BEGIN
				-- Inserting into authentication table
					INSERT INTO TBL_AUTHENTICATION(USERNAME, PASSWORD, EMAIL, ROLEID) VALUES (@USERNAME, @PASSWORD, @EMAIL, @ROLEID);
				-- Getting the id from the authentication table using scope identity function
					DECLARE @AUTHID INT = SCOPE_IDENTITY();

				-- finally inserting into the owner table.
					INSERT INTO TBL_OWNER (FIRST_NAME, MIDDLE_NAME, LAST_NAME, AUTHID, PHONE_NUMBER) VALUES (@FIRSTNAME, @MIDDLENAME, @LASTNAME, @AUTHID, @PHONENUMBER);
				-- AuthResponse for status and message
					SELECT 'SUCCESS' STATUS, 'User created successfully.' MSG;
				END
	END

	--FOR VERIFYING USER EMAIL AND UPDATING ISVERIFY TO TRUE
	IF @FLAG = 'V'
		BEGIN
			UPDATE TBL_AUTHENTICATION
			SET
			ISVERIFIED = 1
			,TOKEN = NULL
			,EXPIRY = NULL
			WHERE
			TOKEN = @TOKEN;
		END



	--FOR INSERTING TOKEN
	IF @FLAG ='I'
		BEGIN
			IF EXISTS (SELECT 1 FROM tbl_authentication WHERE EMAIL = @EMAIL)
			BEGIN
				UPDATE tbl_authentication
				SET TOKEN = @TOKEN,
				EXPIRY = @EXPIRY
				WHERE EMAIL = @EMAIL
			END
		END

	-- FOR Validating TOKEN
	IF @FLAG = 'G'
		BEGIN
			SELECT EMAIL FROM tbl_authentication WHERE TOKEN = @TOKEN AND EXPIRY > GETDATE();
		END

	-- FOR UPDATING EMAIL AND PASSWORD USING TOKEN
IF @FLAG = 'U'
BEGIN
    IF NOT EXISTS (SELECT 1 FROM tbl_authentication WHERE EMAIL = @EMAIL)
    BEGIN
        UPDATE tbl_authentication
        SET 
            PASSWORD = ISNULL(NULLIF(LTRIM(RTRIM(@PASSWORD)), ''), PASSWORD),
            EMAIL = ISNULL(NULLIF(LTRIM(RTRIM(@EMAIL)), ''), EMAIL),
            TOKEN = NULL,
            EXPIRY = NULL,
            PasswordChangeDate = CASE 
                                    WHEN LTRIM(RTRIM(@PASSWORD)) <> '' 
                                    THEN GETDATE() 
                                    ELSE PasswordChangeDate 
                                 END
        WHERE TOKEN = @TOKEN;
    END
END


		
	-- FOR UPDATING PASSWORD
	IF @FLAG = 'P'
		BEGIN
			UPDATE	tbl_authentication	
			SET PASSWORD = @PASSWORD,
			PASSWORDCHANGINGDATE = DATEADD(DAY,30,GETDATE())
			WHERE ID = @ID
		END


END


