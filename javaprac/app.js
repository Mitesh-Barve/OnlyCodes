let add = document.querySelector("#add");
let remove = document.querySelector("#remove");
let inp = document.querySelector("#input");
let ul = document.querySelector('#list');

add.addEventListener("click", function() {
    if (inp.value.trim() === '') {
        // You can add an error message here if needed
    } else {
        let li = document.createElement('li'); // Corrected this line
        li.textContent = inp.value;
        ul.appendChild(li);
        inp.value = "";
    }
});

remove.addEventListener("click", function() {
    let lastLi = ul.lastElementChild; // Get the last li element
    if (lastLi) { // Check if the li exists
        ul.removeChild(lastLi);
    }
});