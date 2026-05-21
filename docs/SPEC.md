# Barkfest — Functional Specification

## Overview

Barkfest is a pet management API allowing owners to register themselves and showcase their pets.

## Actors

- **Owner** — a person who registers an account and manages their pets

## Features

### Owner Management
- Register an owner with first name, last name, email, and optional phone number
- Update owner details
- Delete an owner (cascades to all their pets)
- Upload and remove an owner profile image

### Pet Management
- Add pets to an owner with name, description, date of birth, pet type (Dog/Cat), and breed
- Update pet details
- Delete a pet (cascades to all its images)
- Upload and remove a pet profile image
- Add up to 6 gallery images per pet
- Remove individual gallery images

## Data Storage

- All relational data stored in SQL Server via EF Core
- All images (binary) stored in Azure Blob Storage
- SQL Server holds only image metadata: blob name and content type

## API

RESTful HTTP API. All endpoints return JSON. See [PLAN.md](PLAN.md) for the full endpoint list.

## Constraints

- Image uploads restricted to JPEG, JPG, and PNG formats
- Maximum 6 gallery images per pet
- Pet age is computed from date of birth at runtime — never stored
- Breed is required and must match pet type (Dog → dog breed, Cat → cat breed)
