# Barkfest — Functional Specification

## Overview

Barkfest is a pet management API allowing owners to register themselves and showcase their pets.

## Actors

- **Owner** — a person who registers an account and manages their pets

## Features

### Owner Management
- Register an owner with username, first name, last name, email, and optional phone number
- Update owner details
- Delete an owner (cascades to all their pets)
- Upload and remove an owner profile image (one image per owner)

### Pet Management
- Add pets to an owner with name, optional description, optional date of birth, pet type (Dog/Cat), and breed
- Update pet details
- Delete a pet (cascades to all its images)
- Upload up to 6 images per pet in a single batch request
- Remove an individual image
- Remove multiple images at once via batch delete
- Designate any one image as the featured image (`IsFeaturedImage`)
- The first image uploaded to a pet with no existing images is automatically featured

### Pet Image Rules
- Maximum 6 images total per pet — enforced as a hard domain limit
- Any one image may be designated as the featured image via `PUT /v1/pets/{id}/images/{imageId}/featured`
- Only one image can be featured at a time; designating a new one automatically unfeatures the previous
- Featured is optional — a pet can have images with none featured
- Deleting the featured image leaves the pet with no featured image (no auto-promotion)
- Batch upload: if submitted count exceeds available slots, the entire request is rejected (400) before any processing
- Batch upload: content moderation failures are reported per-image (207); remaining images are saved
- Batch delete: atomic — if any image ID is not found, the entire request is rejected (400)

## Data Storage

- All relational data stored in SQL Server via EF Core
- All images (binary) stored in Azure Blob Storage
- SQL Server holds only image metadata: blob name, content type, display order, and featured flag

## API

RESTful HTTP API. All endpoints return JSON.

### Key endpoints

| Method | Path | Description |
|---|---|---|
| `POST` | `/v1/auth/register` | Register a new owner |
| `POST` | `/v1/auth/login` | Login and receive JWT |
| `GET` | `/v1/owners/{id}` | Get owner by ID |
| `PUT` | `/v1/owners/{id}` | Update owner |
| `DELETE` | `/v1/owners/{id}` | Delete owner |
| `POST` | `/v1/owners/{id}/profile-image` | Upload owner profile image |
| `DELETE` | `/v1/owners/{id}/profile-image` | Remove owner profile image |
| `POST` | `/v1/pets` | Create pet |
| `GET` | `/v1/pets/{id}` | Get pet by ID |
| `PUT` | `/v1/pets/{id}` | Update pet |
| `DELETE` | `/v1/pets/{id}` | Delete pet |
| `POST` | `/v1/pets/{id}/images` | Batch upload images (1 to available slots) |
| `DELETE` | `/v1/pets/{id}/images/{imageId}` | Remove single image |
| `POST` | `/v1/pets/{id}/images/batch-delete` | Batch delete images (atomic) |
| `PUT` | `/v1/pets/{id}/images/{imageId}/featured` | Set featured image |
| `GET` | `/v1/browse/images` | Browse all pet images (public) |

## Constraints

- Image uploads restricted to JPEG, JPG, and PNG formats
- Maximum 6 images per pet (total — no separate profile image concept)
- Pet age is computed from date of birth at runtime — never stored
- Date of birth is optional; the UI may back-calculate it from an age the owner enters
- Breed is required and must match pet type (Dog → dog breed, Cat → cat breed)
- All resource endpoints require a valid owner JWT; ownership enforced in handlers
