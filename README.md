# Unit Tests for Mango .NET Library

This project requeres reference to the https://github.com/mangomessenger/dotnet-mango-library

## Failed tests

1. Send message to direct chat fails, but send to channel / group is ok
2. Get Messages from channel fails, postman - ok, in IDE - fail

## Roadmap

- Authorization api endpoints tested succesfully
- Chat create api endpoints are tested
- Messages endpoint fails on `getChatMessages()` method, other is ok

## Run docker container

1. Get the pgp instance
2. Install composer, make sure to pass folder of php instance
3. Clone laradock to the solution
4. In root folder execute command `composer install`
5. In laradock .env update php version to be 7.4
6. In .env file of root provide proper inner ip you have from `ipconfig` command
7. Run container using `docker-compose up -d nginx mysql phpmyadmin`
8. Initialize seed of db `php artisan migrate:fresh --seed`



