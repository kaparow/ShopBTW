import { CanActivateFn } from '@angular/router';

export interface ProductDto {
  id: number;
  name: string;
  partType: number;
  price: number;
  stock: number;
}
