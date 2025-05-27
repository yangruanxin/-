import { createRouter, createWebHistory } from 'vue-router'

const routes =[
    {
        path:'/',
        name:'首页',
        component:() =>import('../views/Home.vue')
    },

    {
        path:'/login',
        name:'登录',
        component:() =>import('../views/Auth/Login.vue')
    },

    {
        path:'/register',
        name:'注册',
        component:() =>import('../views/Auth/Register.vue')
    },

    {
        path:'/profile',
        name:'个人主页',
        component:() =>import('../views/Auth/Profile.vue')
    },

    {
        path:'/map',
        name:'地图',
        component:() =>import('../views/map.vue')
    }
];

const router = createRouter({
    routes,
    history:createWebHistory(),
})

export default router