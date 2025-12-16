import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, tap } from 'rxjs';
import { AuthResultDto, LoginDto, RegisterDto } from '../models/auth.models';

const TOKEN_KEY = 'shopbtw_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private _token$ = new BehaviorSubject<string | null>(this.getToken());
  token$ = this._token$.asObservable();

  constructor(private http: HttpClient) {}

  register(dto: RegisterDto) {
    return this.http.post<AuthResultDto>('/api/auth/register', dto).pipe(
      tap(res => this.setToken(res.token))
    );
  }

  login(dto: LoginDto) {
    return this.http.post<AuthResultDto>('/api/auth/login', dto).pipe(
      tap(res => this.setToken(res.token))
    );
  }

  logout() {
    localStorage.removeItem(TOKEN_KEY);
    this._token$.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  private setToken(token: string) {
    localStorage.setItem(TOKEN_KEY, token);
    this._token$.next(token);
  }
}
