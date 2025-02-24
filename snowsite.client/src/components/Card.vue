<template>
    <div>
        <div class="container">
            <div class="box" :style="gradientStyle">
                <b class="name">Команата<br><h2>{{ item }}</h2></b>

                <!--<p class="count">{{  item.content }} Участников</p>-->
                <button @click="handleClick">Присоединиться</button>
            </div>
        </div>
    </div>
</template>

<script>
export default {
    props: {
        item: {
            type: String
        },
    },
    data() {
        return {
            gradientStyle: {},
        };
    },
    mounted() {
        this.generateRandomGradient();
    },
    methods: {
        getRandomColor() {
            const letters = '0123456789ABCDEF';
            let color = '#';
            for (let i = 0; i < 6; i++) {
                color += letters[Math.floor(Math.random() * 16)];
            }
            return color;
        },
        generateRandomGradient() {
            const color1 = this.getRandomColor();
            const color2 = this.getRandomColor();
            const angle = Math.floor(Math.random() * 360); // Случайный угол
            this.gradientStyle = {
                background: `linear-gradient(${angle}deg, ${color1}, ${color2})`,
            };
        },
        handleClick() {
      // Отправляем событие с именем 'my-event' родителю
        this.$emit('my-event', this.item.id); // Передаем id элемента
    }


    }
};
</script>

<style scoped>
/* (Ваши стили, как в исходном примере) */

* {
    margin: 0px;
    padding: 0px;
    box-sizing: border-box;
    font-family: "Poppins", serif;
}

body {
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 100vh;
    background: #1d031f;
}

.container {
    position: relative;
    display: flex;
    justify-content: center;
    align-items: center;
    flex-wrap: wrap;
    padding: 50px;
    gap: 50px;
}

.box {
    position: relative;
    height: 400px;
    width: 280px;
    background: #fff;
    /* Fallback */
    border-radius: 20px;
    display: flex;
    /* Added */
    flex-direction: column;
    /* Added */
    justify-content: center;
    /* Added */
    align-items: center;
    /* Added */
    text-align: center;
    /* Added for <p> and <button> */
}

.box::before {
    content: "";
    position: absolute;
    inset: 0;
    border-radius: 20px;
    /* background: #f00; */
    /* Удалите этот background, он будет переопределен */
    background: v-bind(gradientStyle);
}

.box::after {
    content: "";
    position: absolute;
    inset: 0;
    border-radius: 20px;
    /* background: #f00; */
    /* Удалите этот background, он будет переопределен */
    background: v-bind(gradientStyle);
    filter: blur(16px);
}

/* Styles for the overlay */
.box::before,
.box::after {
    z-index: 1;
    /* Place the gradients behind the content */
}

.box b {
    padding: 30px;
    position: absolute;
    /* Changed from absolute to relative */
    display: block;
    inset: 4px;
    border-radius: 16px;
    background: rgba(0, 0, 0, 0.75);
    z-index: 2;

}

.box b p {
    font-weight: 200;

    text-shadow: 0 0 15px #fff;
    z-index: 2;
}

.box button {
    padding: 10px 20px;
    background-color: #70abaf;
    color: #e5dcdc;
    border: none;
    border-radius: 5px;
    cursor: pointer;
    z-index: 2;
}

.name {
    /* Styles for the name */
    padding: 30px;
    position: relative;
    /* Essential for z-index */
    z-index: 2;
    /* Place the name above the gradient */
    border-radius: 16px;
    background: rgba(0, 0, 0, 0.75);

    /* Change to white or a brighter color */
    font-weight: bold;
    /* Make it bold */
    display: block;

    text-shadow: 0 0 5px #fff;
    /* Optional: Add a glow effect */
}

.count {
    /* Styles for the participant count */
    position: relative;
    /* Essential for z-index */
    z-index: 2;
    /* Place the count above the gradient */

    /* Change to a lighter color */
    font-weight: 200;
    text-shadow: 0 0 5px #ddd;
    /* Optional: Add a glow effect */

}
</style>