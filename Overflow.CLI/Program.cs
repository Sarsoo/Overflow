﻿// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;
using Overflow;
using Overflow.SouthernWater;

var driver = new MongoClient("mongodb://localhost");

var api = new SouthernWaterApi(new HttpClient(), NullLogger<SouthernWaterApi>.Instance);
await api.LoadApiUrl();

var runner = new SouthernWaterApiJobRunnerPersisting(api, NullLogger<SouthernWaterApiJobRunner>.Instance, driver.GetDatabase(Static.DatabaseName));

await runner.LoadSpills(5);