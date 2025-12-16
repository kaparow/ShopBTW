import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AddToCartDto, CartDto, CheckoutResultDto } from '../models/cart.models';

@Injectable({ providedIn: 'root' })
export class CartService {
  constructor(private http: HttpClient) { }

  getCart() {
    return this.http.get<CartDto>('/api/cart');
  }

  addItem(dto: AddToCartDto) {
    return this.http.post<CartDto>('/api/cart/items', dto);
  }

  checkout() {
    return this.http.post<CheckoutResultDto>('/api/cart/checkout', {});
  }
  
  updateItem(productId: number, quantity: number) {
    return this.http.put<CartDto>('/api/cart/items', { productId, quantity });
  }

  removeItem(productId: number) {
    return this.http.delete<CartDto>('/api/cart/items', {
      params: { productId: productId.toString() }
    });
  }


}
