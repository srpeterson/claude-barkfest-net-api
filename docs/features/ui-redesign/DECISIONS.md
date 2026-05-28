# UI Redesign — Decisions

## D1: Post a Pet stays as inline AddPetDialog

The floating pill Navbar has a "+" button that opens AddPetDialog inline — not a /add-pet route.
Same behaviour as the current Navbar. Confirmed by user.

## D2: api.ts core is frozen; endpoint functions are allowed

The core infrastructure (request(), setAuthToken(), setUnauthorizedHandler(), ApiError, api object)
must not be modified. New endpoint functions (getPetDetail, getOwnerPets, deletePet, updatePet,
changePassword) may be added to api.ts. Confirmed by user.

## D3: Split-panel auth pages navigate to / on success

Both LoginPage and RegisterPage navigated to /owners on success (legacy leftover).
Redesign changes both to navigate('/') after sign-in/register.
