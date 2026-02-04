import { UserResponse } from './UserResponseModel';

export interface AuthResult {
  token: string;
  expires: Date;
  user: UserResponse;
}