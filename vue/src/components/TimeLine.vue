<template>
    <div
      ref="timelineContainerRef"
      class="w-full bg-white font-sans md:px-10 dark:bg-neutral-950"
    >
  
      <div ref="timelineRef" class="relative z-0 w-full pb-20">
        <div
          v-for="(item, index) in props.items"
          :key="item.id + index"
          class="flex justify-start  md:pt-20 mb-20"
        >
          <!-- 时间点左侧内容 -->
          <div
            class="relative top-40 z-40 flex max-w-xs flex-col items-center self-start lg:max-w-sm md:w-full md:flex-row"
          >
            <div
              class="absolute left-3 flex size-10 items-center justify-center rounded-full bg-white md:left-3 dark:bg-black"
            >
              <div
                class="size-4 rounded-full border border-neutral-300 bg-neutral-200 p-2 dark:border-neutral-700 dark:bg-neutral-800"
              />
            </div>
          </div>
          <div class="w-full mt-16 md:mt-20">
            <slot :name="item.id"></slot>
          </div>
        </div>
  
        <!-- 动画线 -->
        <div
          :style="{ height: height + 'px' }"
          class="absolute left-8 top-0 w-[2px] overflow-hidden bg-[linear-gradient(to_bottom,var(--tw-gradient-stops))] from-transparent from-0% via-neutral-200 to-transparent to-[99%] [mask-image:linear-gradient(to_bottom,transparent_0%,black_10%,black_90%,transparent_100%)] md:left-8 dark:via-neutral-700"
        >
          <Motion
            as="div"
            :style="{ height: animatedHeight, opacity: animatedOpacity }"
            class="absolute inset-x-0 top-0 w-[2px] rounded-full bg-gradient-to-t from-purple-500 from-0% via-blue-500 via-10% to-transparent"
          />
        </div>
      </div>
    </div>
  </template>
  
  <script lang="ts" setup>
  import { ref, onMounted, nextTick, watch } from 'vue';
  import { Motion, useScroll, useTransform, useMotionValueEvent } from 'motion-v';

  onMounted(async () => {
    await nextTick();
    if (timelineRef.value) {
      height.value = timelineRef.value.scrollHeight;
    }
  });


  interface Props {
    containerClass?: string;
    class?: string;
    items?: { id: string; label: string }[];
    title?: string;
    description?: string;
  }

  const props = withDefaults(defineProps<Props>(), {
    items: () => [],
  });

  const timelineRef = ref<HTMLElement | null>(null);
  const height = ref(0);

  const { scrollYProgress } = useScroll({
    target: timelineRef,
    offset: ['start 10%', 'end 100%'],
  });

  const opacityTransform = useTransform(scrollYProgress, [0, 0.1], [0, 1]);
  const animatedOpacity = ref(0);
  useMotionValueEvent(opacityTransform, 'change', (latest) => {
    animatedOpacity.value = latest;
  });

  // 先声明 heightTransform 变量，稍后赋值
  let heightTransform = useTransform(scrollYProgress, [0, 1], [0, 0]);
  const animatedHeight = ref('0px');

  // 监听高度变化，更新 heightTransform 范围
  watch(height, (newHeight) => {
    // 先解绑之前的事件监听
    // 创建新的 transform，绑定监听事件更新 animatedHeight
    heightTransform = useTransform(scrollYProgress, [0, 1], [0, newHeight]);
    useMotionValueEvent(heightTransform, 'change', (latest) => {
      animatedHeight.value = latest + 'px';
    });
  });


  </script>
