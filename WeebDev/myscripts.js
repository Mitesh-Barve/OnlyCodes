let cartQuantity = 0;

function showQuantity() {
    console.log(`Cart quantity: ${cartQuantity}`);
}

function incrementQuantity(amount) {
    cartQuantity += amount;
    console.log(`Cart quantity: ${cartQuantity}`);
}

function resetCart() {
    cartQuantity = 0;
    console.log('Cart was reset.');
    showQuantity();
}
