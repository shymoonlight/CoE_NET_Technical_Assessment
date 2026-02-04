import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthResult } from '../models/AuthResultModel';
import { LoginRequest } from '../models/LoginRequestModel';



@Injectable({ providedIn: 'root' })
export class LoginService {
  private readonly url = 'https://localhost:7189/api/login';

  constructor(private http: HttpClient) {}

  login(payload: LoginRequest): Observable<AuthResult> {
    const httpOptions = {
        headers: new HttpHeaders ({
            "Access-Control-Allow-Origin": "**"
        })
}
    return this.http.post<AuthResult>(this.url, payload, httpOptions);
  }
}