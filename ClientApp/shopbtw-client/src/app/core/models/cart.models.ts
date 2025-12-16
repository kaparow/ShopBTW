import { CanActivateFn } from '@angular/router';

export interface AddToCartDto {
  productId: number;
  quantity: number;
}

export interface CartItemDto {
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface CartDto {
  id: number;
  customerId: number;
  total: number;
  items: CartItemDto[];
}

export interface CheckoutResultDto {
  orderId: number;
  createdAt: string;
  total: number;
}