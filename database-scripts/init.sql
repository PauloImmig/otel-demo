USE oteldemo;

CREATE TABLE WeatherForecastRequests(
    Id INT NOT NULL AUTO_INCREMENT,
    ResponseBody VARCHAR(4000),
    RequestedAt DATETIME,
    PRIMARY KEY (Id)
)