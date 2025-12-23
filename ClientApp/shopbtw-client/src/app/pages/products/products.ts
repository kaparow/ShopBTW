import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { finalize } from 'rxjs';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { ViewChild } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import { ProductsService } from '../../core/services/products.service';
import { CartService } from '../../core/services/cart.service';
import { AuthService } from '../../core/services/auth.service';
import { ProductDto } from '../../core/models/product.models';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule,
    MatProgressSpinnerModule, MatTableModule, MatSortModule],
  templateUrl: './products.html',
  styleUrl: './products.scss',
})
export class ProductsComponent implements OnInit {
  products: ProductDto[] = [];
  loading = false;
  needLogin = false;
  error = '';

  // выбранные количества по товарам
  qty: Record<number, number> = {};

  constructor(
    private productsApi: ProductsService,
    private cartApi: CartService,
    public auth: AuthService,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) { }

  viewMode: 'cards' | 'table' = (localStorage.getItem('shopbtw_view') as any) ?? 'cards';

  displayedColumns: string[] = ['name', 'type', 'price', 'actions'];
  dataSource = new MatTableDataSource<ProductDto>([]);

  @ViewChild(MatSort) set matSort(sort: MatSort) {
    this.dataSource.sort = sort;
  }

  setView(mode: 'cards' | 'table') {
    this.viewMode = mode;
    localStorage.setItem('shopbtw_view', mode);
    this.cdr.detectChanges();
  }

  ngOnInit(): void {
    this.loadProducts();
  }
  goToLogin(): void {
    this.router.navigate(['/login'], { queryParams: { returnUrl: '/' } });
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
          this.dataSource.data = r;
          // если у товара ещё нет qty — ставим 1
          this.dataSource.sortingDataAccessor = (item, prop) => {
            switch (prop) {
              case 'type': return this.partTypeName(item.partType);
              case 'price': return item.price;
              case 'name': return item.name;
              default: return (item as any)[prop];
            }
          }
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
    this.needLogin = false;

    const quantity = this.qty[productId] ?? 1;

    this.cartApi.addItem({ productId, quantity }).subscribe({
      next: () => {
        this.cdr.detectChanges();
      },
      error: (e) => {
        if (e?.status === 401) {
          this.needLogin = true;
          this.error = 'Для добавления в корзину необходимо авторизоваться';
        } else {
          this.needLogin = false;
          this.error = (e?.error ?? e?.message ?? 'Ошибка добавления в корзину').toString();
        }
        this.cdr.detectChanges();
      }
    });
  }

  trackById(_: number, p: ProductDto): number {
    return p.id;
  }
  partTypeName(partType: number): string {
    const map: Record<number, string> = {
      1: 'Гусеницы',
      2: 'Двигатель',
      3: 'Контроллер',
      4: 'Аккумулятор',
      5: 'Колесо',
      6: 'Робот',
    };

    return map[partType] ?? `Тип ${partType}`;
  }

}
