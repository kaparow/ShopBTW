import { CanActivateFn } from '@angular/router';

export interface RegisterDto {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface AuthResultDto {
  customerId: number;
  email: string;
  token: string;
  expiresAtUtc: string;
}
