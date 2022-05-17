namespace OrderService.GraphQL
{
    public class StatusOrder
    {
        public static readonly string Waiting = "MENUNGGU KONFIRMASI RESTORAN";
        public static readonly string OnProses = "DALAM PROSES PEMBUATAN";
        public static readonly string OnDelivery = "DALAM PROSES PENGANTARAN";
        public static readonly string Completed = "PESANAN SELESAI";
    }
}
