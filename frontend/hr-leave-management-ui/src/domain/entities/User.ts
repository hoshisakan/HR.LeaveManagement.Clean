/**
 * User (Employee) model - aligns with backend UsersController / Employee DTO.
 * GET /api/v1/Users returns list of these; GET /api/v1/Users/{id} returns one.
 */
export interface User {
  id: string
  email: string
  firstName: string
  lastName: string
}
