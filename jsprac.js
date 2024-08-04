class animal{
    constructor(name , legcount , speaks){
        this.name = name ;
        this.legcount = legcount ;
        this.speacks = speaks;
    }
    speacks(){
        console.log("hi there"+ this.speaks);
    }
}
let dog = new animal("dog",4,"bhow bhow");
let cat = new animal("cat",4,"meow meow");
cat.speaks();
/********************************************************************/
/Callback function/
function square(a) {
    return a * a;
}

function sumOfSomething(a, b, fn) {
    const val1 = fn(a);
    const val2 = fn(b);
    return val1 + val2;
}

const a = 3;
const b = 4;
console.log(sumOfSomething(a, b, square)); // Output: 25
