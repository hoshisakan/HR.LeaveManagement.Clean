import { apiClient } from '../apiClient'
import type { User } from '../../domain/entities/User'

const USERS_BASE = '/api/v1/Users'

/** Raw API response - backend may return PascalCase or camelCase */
type UserDto = Record<string, unknown> & {
  id?: string
  Id?: string
  email?: string
  Email?: string
  firstName?: string
  FirstName?: string
  lastName?: string
  LastName?: string
}

function toUser(dto: UserDto): User {
  return {
    id: dto.id ?? dto.Id ?? '',
    email: dto.email ?? dto.Email ?? '',
    firstName: dto.firstName ?? dto.FirstName ?? '',
    lastName: dto.lastName ?? dto.LastName ?? '',
  }
}

/**
 * User service - mirrors backend UsersController.
 * - GetUsers() -> GET /api/v1/Users
 * - GetUser(id) -> GET /api/v1/Users/{id}
 */
export const userService = {
  async getUsers(): Promise<User[]> {
    const list = await apiClient<UserDto[]>(USERS_BASE)
    return list.map(toUser)
  },

  async getUser(id: string): Promise<User> {
    const dto = await apiClient<UserDto>(`${USERS_BASE}/${id}`)
    return toUser(dto)
  },
}
