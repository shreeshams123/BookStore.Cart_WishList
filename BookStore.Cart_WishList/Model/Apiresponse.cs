namespace BookStore.Cart_WishList.Model
{
    public class Apiresponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
