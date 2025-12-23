import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { CartDto } from '../../core/models/cart.models';
import { finalize } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';


@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatProgressSpinnerModule,],
  templateUrl: './cart.html',
  styleUrls: ['./cart.scss'],
})
export class CartComponent implements OnInit {
  cart: CartDto = {
    id: 0,
    customerId: 0,
    total: 0,
    items: [],
  };
  loading = false;
  error = '';

  constructor(private cartApi: CartService, private cdr: ChangeDetectorRef, private router: Router ) { }

  ngOnInit() {
    console.log('CartComponent init called');
    this.loadCart();
  }

  private runCartRequest(obs$: Observable<CartDto>) {
    this.loading = true;
    this.error = '';
    this.cdr.detectChanges();

    obs$
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: (c) => {
          this.cart = c;
          this.cdr.detectChanges();
        },
        error: (e) => {
          this.cart = this.cart; // можно не трогать
          this.error = (e?.error ?? e?.message ?? 'Ошибка').toString();
          this.cdr.detectChanges();
        }
      });
  }

  loadCart() {
    this.runCartRequest(this.cartApi.getCart());
  }

  increase(productId: number, currentQty: number) {
    this.runCartRequest(this.cartApi.updateItem(productId, currentQty + 1));
  }

  decrease(productId: number, currentQty: number) {
    const nextQty = currentQty - 1;
    if (nextQty <= 0) return this.remove(productId);
    this.runCartRequest(this.cartApi.updateItem(productId, nextQty));
  }

  remove(productId: number) {
    this.runCartRequest(this.cartApi.removeItem(productId));
  }

  checkout() {
    if (!this.cart || this.cart.items.length === 0) return;

    this.loading = true;
    this.error = '';

    this.cartApi.checkout().subscribe({
      next: (res) => {
        this.loading = false;
        alert(`Заказ оформлен! Номер заказа: ${res.orderId}, Сумма: ${res.total}`);
        this.loadCart();
      },
      error: (e) => {
        this.loading = false;
        this.error = (e?.error ?? e?.message ?? 'Не удалось оформить заказ').toString();
      }
    });
  }
}
