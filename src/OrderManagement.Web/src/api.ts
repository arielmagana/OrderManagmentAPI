export type ApiError = { type: string; title: string; status: number; detail: string; code: string; errors?: Record<string, string[]> };
export type Page<T> = { pageNumber: number; pageSize: number; totalCount: number; totalPages: number; items: T[] };
export type Customer = { id:number; name:string; email:string; isActive:boolean };
export type Product = { id:number; sku:string; name:string; unitPrice:number; isActive:boolean };
export type OrderItem = { productId:number; quantity:number; unitPrice:number; subtotal:number };
export type Order = { id:number; customerId:number; orderDate:string; status:OrderStatus; totalAmount:number; items:OrderItem[] };
export type OrderStatus = 'Pending'|'Confirmed'|'Completed'|'Cancelled';
export type Health = { status:string; checks:{name:string;status:string}[] };
const base = (import.meta.env.VITE_API_BASE_URL || '').replace(/\/$/, '');
async function request<T>(path:string, init?:RequestInit):Promise<T> {
  const response = await fetch(`${base}${path}`, { ...init, headers:{'Content-Type':'application/json',...init?.headers} });
  if (!response.ok) { let error:ApiError; try { error=await response.json(); } catch { error={type:'about:blank',title:'Request failed',status:response.status,detail:'The service returned an unexpected response.',code:'unexpected_response'}; } throw error; }
  return response.json();
}
const query=(values:Record<string,string|number|undefined>)=>new URLSearchParams(Object.entries(values).filter(([,v])=>v!==undefined).map(([k,v])=>[k,String(v)])).toString();
export const api = {
 health:()=>request<Health>('/api/health'),
 customers:(page=1,pageSize=12)=>request<Page<Customer>>(`/api/customers?${query({page,pageSize})}`),
 createCustomer:(body:object)=>request<Customer>('/api/customers',{method:'POST',body:JSON.stringify(body)}),
 products:(page=1,pageSize=12)=>request<Page<Product>>(`/api/products?${query({page,pageSize})}`),
 createProduct:(body:object)=>request<Product>('/api/products',{method:'POST',body:JSON.stringify(body)}),
 orders:(page=1,pageSize=10,customerId?:number,status?:string)=>request<Page<Order>>(`/api/orders?${query({page,pageSize,customerId,status})}`),
 order:(id:number)=>request<Order>(`/api/orders/${id}`),
 createOrder:(body:object)=>request<Order>('/api/orders',{method:'POST',body:JSON.stringify(body)}),
 changeStatus:(id:number,status:OrderStatus)=>request<Order>(`/api/orders/${id}/status`,{method:'PUT',body:JSON.stringify({status})})
};
