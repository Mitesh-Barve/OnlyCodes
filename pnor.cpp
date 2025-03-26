#include<iostream>
using namespace std;

int main() {
    int n, i;
    cout << "Enter a number: ";
    cin >> n;

    // If the number is less than 2, it's not prime
    if (n < 2) {
        cout << "Not a prime number";
        return 0;
    }
    else {
        // Check for factors of n from 2 to n-1
        for (i = 2; i < n; i++) {
            if (n % i == 0) {
                cout << "Not a prime number";
                return 0;
            }
        }
    }

    // If no factors were found, it's a prime number
    cout << "Prime number";
    return 0;
}
