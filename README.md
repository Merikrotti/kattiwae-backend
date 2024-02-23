# cryptogram-backend and the SSO software
Backend of cryptogram. Config.yaml file for PostgreSQL authentication.
The SSO service is in KattiSSO/
The interesting bits are ofcourse in Controllers, but most of the logic or database usage is in Project/Services/ They are used as singletons (in either Startup.cs) for ServiceCollection dependency injection.

Most interesting bits are JWT authentication and using SQL for PSQL Database when Entity Framework wasn't available for it.

There were more commits, but Windows 11 blue screen corrupted the drive.
Part of the front end is at  https://github.com/Merikrotti/kattisso
The page was hosted in https://kattiwae.com/ now taken offline.
