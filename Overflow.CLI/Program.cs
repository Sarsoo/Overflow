// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;
using Overflow;
using Overflow.SouthernWater;

var driver = new MongoClient("mongodb://localhost");

var api = new SouthernWaterApi(new HttpClient());
await api.LoadApiUrl();

var runner = new SouthernWaterApiJobRunnerPersisting(api, NullLogger<SouthernWaterApiJobRunner>.Instance, driver.GetDatabase("overflow"));

await runner.LoadSpills(5);