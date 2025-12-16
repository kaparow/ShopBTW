import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { finalize } from 'rxjs';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { ProductsService } from '../../core/services/products.service';
import { CartService } from '../../core/services/cart.service';
import { AuthService } from '../../core/services/auth.service';
import { ProductDto } from '../../core/models/product.models';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatProgressSpinnerModule],
  templateUrl: './products.html',
  styleUrl: './products.scss',
})
export class ProductsComponent implements OnInit {
  products: ProductDto[] = [];
  loading = false;
  error = '';

  // выбранные количества по товарам
  qty: Record<number, number> = {};

  constructor(
    private productsApi: ProductsService,
    private cartApi: CartService,
    public auth: AuthService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.loading = true;
    this.error = '';
    this.cdr.detectChanges();

    this.productsApi.getAll()
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: (r) => {
          this.products = r;
          // если у товара ещё нет qty — ставим 1
          for (const p of r) {
            if (!this.qty[p.id]) this.qty[p.id] = 1;
          }
          this.cdr.detectChanges();
        },
        error: (e) => {
          this.error = (e?.error ?? e?.message ?? 'Failed to load products').toString();
          this.cdr.detectChanges();
        },
      });
  }

  increaseQty(productId: number): void {
    this.qty[productId] = (this.qty[productId] ?? 1) + 1;
    this.cdr.detectChanges();
  }

  decreaseQty(productId: number): void {
    const cur = this.qty[productId] ?? 1;
    this.qty[productId] = Math.max(1, cur - 1);
    this.cdr.detectChanges();
  }

  addToCart(productId: number): void {
    this.error = '';
    const quantity = this.qty[productId] ?? 1;

    this.cartApi.addItem({ productId, quantity }).subscribe({
      next: () => {
        // можно показать сообщение, но хотя бы перерисуем
        this.cdr.detectChanges();
      },
      error: () => {
        this.error = 'Для использования корзины, необходимо авторизоваться';
        this.cdr.detectChanges();
      }
    });
  }

  trackById(_: number, p: ProductDto): number {
    return p.id;
  }
}
