#!/bin/bash
dotnet ef "$@" \
  --project src/TaskManagement.Infrastructure \
  --startup-project src/TaskManagement.API