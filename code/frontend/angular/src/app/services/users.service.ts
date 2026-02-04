import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { UserResponse } from '../models/UserResponseModel';
import { UserDto, UserRequestDto } from '../models/UserDtoModel';


@Injectable({ providedIn: 'root' })
export class UsersService {
  private readonly baseUrl = 'https://localhost:7189/api/user';

  constructor(private http: HttpClient) {}

  getUsers(): Observable<UserDto[]> {
    return this.http.get<UserResponse[]>(this.baseUrl).pipe(
      map((users) =>
        users.map((user) => ({
          id: String(user.id),
          name: user.name,
          userName: user.userName,
          email: user.email,
          role: user.role,
          createDate: user.creationDate ? new Date(user.creationDate).toLocaleString() : undefined,
          lastUpdateDate: user.lastUpdateDate ? new Date(user.lastUpdateDate).toLocaleString() : undefined,
        }))
      )
    );
  }

  createUser(payload: UserRequestDto): Observable<UserDto> {
    return this.http.post<UserDto>(this.baseUrl, payload);
  }

  updateUser(id: string, payload: UserRequestDto): Observable<UserDto> {
    return this.http.put<UserDto>(`${this.baseUrl}/${id}`, payload);
  }

  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}