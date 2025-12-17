import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { finalize } from 'rxjs';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';

import { OrdersService } from '../../core/services/orders.service';
import { OrderDto } from '../../core/models/order.models';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatProgressSpinnerModule, MatDividerModule],
  templateUrl: './orders.html',
  styleUrl: './orders.scss',
})
export class OrdersComponent implements OnInit {
  orders: OrderDto[] = [];
  loading = false;
  error = '';

  constructor(private ordersApi: OrdersService, private cdr: ChangeDetectorRef) { }

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading = true;
    this.error = '';
    this.cdr.detectChanges();

    this.ordersApi.getMyOrders()
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: (r) => {
          this.orders = r;
          this.cdr.detectChanges();
        },
        error: (e) => {
          this.error = (e?.error ?? e?.message ?? 'Не удалось загрузить заказы').toString();
          this.cdr.detectChanges();
        }
      });
  }
}
