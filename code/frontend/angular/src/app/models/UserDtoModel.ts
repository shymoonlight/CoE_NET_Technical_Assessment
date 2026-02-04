export interface UserDto {
  id: string;
  name: string;
  userName: string;
  email: string;
  role: string;
  createDate?: string;
  lastUpdateDate?: string;
}

export interface UserRequestDto {
  name: string;
  email: string;
  password: string;
  userName: string;
  role: string;
}
