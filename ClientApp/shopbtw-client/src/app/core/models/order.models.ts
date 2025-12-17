export interface OrderItemDto {
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface OrderDto {
  id: number;
  createdAt: string;
  total: number;
  items: OrderItemDto[];
}
