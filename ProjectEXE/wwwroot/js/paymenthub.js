const connection = new signalR.HubConnectionBuilder()
    .withUrl("/paymentHub", { accessTokenFactory: () => token })
    .build();

connection.on("PaymentSuccess", (packageId) => {
    alert("Gói " + packageId + " đã được kích hoạt!");
});

connection.start();
