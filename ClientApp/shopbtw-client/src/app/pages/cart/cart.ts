import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { CartDto } from '../../core/models/cart.models';
import { finalize } from 'rxjs/operators';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './cart.html',
  styleUrls: ['./cart.scss'],
})
export class CartComponent implements OnInit {
  cart: CartDto | null = null;
  loading = false;
  error = '';

  constructor(private cartApi: CartService, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    console.log('CartComponent init called');
    this.loadCart();
  }

  loadCart() {
    this.loading = true;
    this.error = '';
    this.cdr.detectChanges();

    this.cartApi.getCart().subscribe({
      next: (c) => {
        this.cart = c;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (e) => {
        this.cart = null;
        this.loading = false;
        this.error = (e?.error ?? e?.message ?? 'Не удалось загрузить корзину').toString();
        this.cdr.detectChanges();
      }
    });
  }

  increase(productId: number, currentQty: number) {
    this.loading = true;
    this.cartApi.updateItem(productId, currentQty + 1).subscribe({
      next: (c) => { this.cart = c; this.loading = false; },
      error: (e) => { this.error = (e?.error ?? e?.message ?? 'Ошибка обновления').toString(); this.loading = false; }
    });
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

  decrease(productId: number, currentQty: number) {
    const nextQty = currentQty - 1;
    if (nextQty <= 0) return this.remove(productId);

    this.loading = true;
    this.cartApi.updateItem(productId, nextQty).subscribe({
      next: (c) => { this.cart = c; this.loading = false; },
      error: (e) => { this.error = (e?.error ?? e?.message ?? 'Ошибка обновления').toString(); this.loading = false; }
    });
  }

  remove(productId: number) {
    this.loading = true;
    this.cartApi.removeItem(productId).subscribe({
      next: (c) => { this.cart = c; this.loading = false; },
      error: (e) => { this.error = (e?.error ?? e?.message ?? 'Ошибка удаления').toString(); this.loading = false; }
    });
  }

  checkout() {
    if (!this.cart || this.cart.items.length === 0) return;

    this.loading = true;
    this.error = '';

    this.cartApi.checkout().subscribe({
      next: (res) => {
        this.loading = false;
        alert(`Заказ оформлен! OrderId: ${res.orderId}, Total: ${res.total}`);
        this.loadCart();
      },
      error: (e) => {
        this.loading = false;
        this.error = (e?.error ?? e?.message ?? 'Не удалось оформить заказ').toString();
      }
    });
  }
}
