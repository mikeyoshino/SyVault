#!/usr/bin/env dotnet-script
#r "nuget: Otp.NET, 1.4.0"

using OtpNet;

var secret = Args[0];
var secretBytes = Base32Encoding.ToBytes(secret);
var totp = new Totp(secretBytes);
var code = totp.ComputeTotp();
Console.WriteLine(code);
