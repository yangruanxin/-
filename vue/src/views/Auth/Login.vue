<!-- 登录页面 -->
<template>
  <div class="login-body">
    <div class="login-form">
        <div class="login-image">
            <img src="/assets/图标.jpeg" alt="登录背景图">
        </div>
        <el-form
            ref="ruleFormRef"
            :model="LoginForm"
            :rules="rules"
            label-width="auto"
        >
            <p> 继续你的旅程吧！</p>
            <br>

            <!-- 账号框 -->
            <el-form-item label="账号" prop="account">
                <el-input  placeholder="请输入您的账号" v-model="LoginForm.account"/>
            </el-form-item>

            <!-- 密码框 -->
            <el-form-item label="密码" prop="password">
                <el-input  placeholder="请输入您的密码" type="password" v-model="LoginForm.password" show-password/>
            </el-form-item>

            <!-- 登录和取消按钮 -->
            <el-form-item label="" class="button-group">
                <div class="button-row">
                    <el-button type="primary" class="login-btn" @click="handleLogin"   :loading="loading" :disabled="loading">{{ loading ? '登录中...' : '登录' }}</el-button>
                    <el-button type="primary" class="cancel-btn" @click="handleCancel" :disabled="loading">取消</el-button>
                </div>

                <div class="register-link">
                  还没有账号？
                  <el-button type="primary" @click="goToRegister" :disabled="loading" style="padding: 0; margin-left: 5px;">立即注册</el-button>
                </div>
            </el-form-item>
        </el-form>
    </div>
  </div>
</template>

<script setup>

import {reactive,ref} from "vue"
import {useRouter} from "vue-router"
import { publicAxios } from "@/utils/request"
import { ElMessage } from 'element-plus'
import { useAuthStore } from '@/stores/auth'

const loading = ref(false) // 控制加载状态
const authStore = useAuthStore()

const LoginForm = reactive({
  account: '',
  password: ''
})

const ruleFormRef = ref()

const rules = {
  account: [{ required: true, message: '请输入账号', trigger: 'blur' }],
  password: [{ required: true, message: '请输入密码', trigger: 'blur' }]
}

//获取路由事例
const router=useRouter()

// 登录按钮点击事件
const handleLogin = () => {
  ruleFormRef.value.validate(async (valid) => {
    if (valid) {
      loading.value = true // 开始加载，禁用按钮
      try {
        const response = await publicAxios.post('user/login', {
          username: LoginForm.account,
          password: LoginForm.password
        }, {
          headers: {
            'Content-Type': 'application/json'
          }
        })

        if (response.data.code === 200) {
          ElMessage.success('登录成功！')
          // 使用authStore的login方法保存token和状态
          authStore.login(response.data.data.token)
          console.log(response.data.data.token)
          // 跳转到主页
          router.push('/')
        } else {
          ElMessage.error(response.data.message || '登录失败')
        }
      } catch (error) {
        console.error(error)
        ElMessage.error('网络错误或服务器异常')
      } finally {
        loading.value = false // 无论成功失败，都结束加载
      }
    }
  })
}


//“取消”按钮点击事件
const handleCancel = () =>{
    router.push('/')
}

// 跳转注册页面
const goToRegister = () => {
  router.push('/register')
}

</script>

<style lang="css" scoped>

.login-body:before{
    content: "";
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background-image:url("/assets/背景1.jpeg") ;
    background-size: cover;
    background-position: center;/* 居中 */
    filter: brightness(70%); /* 调整亮度 */
    z-index: -1; /* 让背景在最底层 */
}

.login-body{
    display: grid;
    place-items: center;
    min-height: 100vh;
}

.login-form{
    display: grid;
    grid-template-columns: 1fr 1fr; /* 两列等宽布局 */
    align-items: center; /* 垂直居中 */
    gap: 40px; /* 图片和表单之间的间距 */
    background-color: rgba(255, 255, 255, 0.8);
    background-position: center;
    padding: 8rem;
    border-radius: 20px;
    width: 800px;
}

.login-image {
    display: flex;
    justify-content: center;
}

.login-image img {
    max-width: 100%;
    max-height: 600px;
    object-fit: contain;
    border-radius: 8px;
}

.el-button{
    color:white;
    background-color: #0aa3fb;
    border-radius: 8px;
    width: 100%;
}

.el-input{
    border-radius: 8px;
}


p{
    font-size: 150%;
    font-weight: bold;
}

.button-group {
  margin-top: 20px;
  display: flex;
  justify-content: flex-end; 
  align-items: center;
  width: 100%;
}

.button-row {
    display: flex;
    gap: 10px; /* 按钮之间的间距 */
    width: 100%;
}

.register-link {
  margin-top: 15px;
  display: flex;
  align-items: center;
  gap: 5px;
  font-size: 14px;
  color: #666;
  white-space: nowrap;
  width: 100%;
}

.login-btn, .cancel-btn {
    flex: 1; /* 使两个按钮等宽 */
}

</style>
