import type { ApiError as ApiErrorType, Order, OrderStatus } from './api';
export function ApiError({error}:{error:ApiErrorType}) { return <div className="error" role="alert"><strong>{error.title}</strong><p>{error.detail}</p>{error.errors&&<ul>{Object.entries(error.errors).flatMap(([field,messages])=>messages.map(message=><li key={field+message}><b>{field}:</b> {message}</li>))}</ul>}<small>Reference: {error.code}</small></div> }
export function Pagination({page,totalPages,onChange}:{page:number,totalPages:number,onChange:(page:number)=>void}) { if(totalPages<=1)return null; return <nav className="pagination" aria-label="Pagination"><button disabled={page<=1} onClick={()=>onChange(page-1)}>Previous</button><span>Page {page} of {totalPages}</span><button disabled={page>=totalPages} onClick={()=>onChange(page+1)}>Next</button></nav> }
export const orderTotal=(order:Pick<Order,'items'>)=>order.items.reduce((sum,item)=>sum+item.quantity*item.unitPrice,0);
export const validStatusActions=(status:OrderStatus):OrderStatus[]=>status==='Pending'?['Confirmed','Cancelled']:status==='Confirmed'?['Completed']:[];
export function Skeleton(){return <div className="skeleton" aria-label="Loading" aria-busy="true"><i/><i/><i/></div>}
export function Empty({children}:{children:string}){return <div className="empty"><strong>Nothing here yet</strong><p>{children}</p></div>}
