import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductsService } from '../../core/services/products.service';
import { CartService } from '../../core/services/cart.service';
import { AuthService } from '../../core/services/auth.service';
import { RouterLink } from '@angular/router';
import { ProductDto } from '../../core/models/product.models';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div style="display:flex; gap:12px; align-items:center;">
      <a routerLink="/">Товары</a>
      <a routerLink="/cart">Корзина</a>
      <a routerLink="/orders">Заказы</a>
      <a routerLink="/login" *ngIf="!auth.isLoggedIn()">Login</a>
      <button *ngIf="auth.isLoggedIn()" (click)="logout()">Logout</button>
    </div>

    <h2>Товары</h2>

    <div *ngFor="let p of products" style="border:1px solid #ddd; padding:12px; margin:8px 0;">
      <div><b>{{p.name}}</b></div>
      <div>Цена: {{p.price}}</div>
      <div>Остаток: {{p.stock}}</div>

      <button (click)="addToCart(p.id)">Добавить в корзину</button>
    </div>

    <p style="color:#c00" *ngIf="error">{{error}}</p>
  `
})
export class ProductsComponent {
  products: ProductDto[] = [];
  error = '';

  constructor(
    private productsApi: ProductsService,
    private cartApi: CartService,
    public auth: AuthService
  ) {
    this.productsApi.getAll().subscribe({
      next: r => this.products = r,
      error: e => this.error = (e?.error ?? e?.message ?? 'Failed to load products').toString()
    });
  }

  addToCart(productId: number) {
    this.error = '';
    this.cartApi.addItem({ productId, quantity: 1 }).subscribe({
      next: c => alert(`Добавлено. Total: ${c.total}`),
      error: e => this.error = 'Нужен логин (JWT). Перейди на /login и войди.'
    });
  }

  logout() {
    this.auth.logout();
  }
}
