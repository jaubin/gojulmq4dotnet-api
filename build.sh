#!/bin/bash
MSBuildEmitSolution=1 dotnet build $(dirname $0)/gojulmq4dotnet.sln
