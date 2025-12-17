import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { OrderDto } from '../models/order.models';

@Injectable({ providedIn: 'root' })
export class OrdersService {
  constructor(private http: HttpClient) {}

  getMyOrders() {
    return this.http.get<OrderDto[]>('/api/orders');
  }
}
