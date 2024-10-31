using Domain;

var db = new TeamDbContext();
db.Database.EnsureCreated();